#pragma once

#include "muctx.h"
#include "Cache.h"
#include <mutex>
extern "C" {
#include "mupdf/pdf/event.h"
}

/* This class interfaces to mupdf API with minimal windows objects
 * (other than the file streaming stuff) */
static int textlen(fz_stext_page *page);

#ifdef _WINRT_DLL
// Attempt to use t.wait()
//#include <ppltasks.h>
//using namespace concurrency;
/* File streaming set up for WinRT */
static int win_next_file(fz_context *ctx, fz_stream *stm, size_t len)
{
	void *temp = stm->state;
	win_stream_struct *state = reinterpret_cast <win_stream_struct*> (temp);
	IRandomAccessStream^ Stream = state->stream;
	unsigned char *buf = state->public_buffer;
	unsigned long long curr_pos = Stream->Position;
	unsigned long long length = Stream->Size;
	DataReader^ local_reader = ref new DataReader(Stream);
	if (local_reader == nullptr)
		return 0;

	DataReaderLoadOperation^ result = local_reader->LoadAsync(len);
	while (result->Status != AsyncStatus::Completed) {
	}
	result->GetResults();

	/* First see what is available */
	int curr_len2 = local_reader->UnconsumedBufferLength;
	if (curr_len2 < len)
		len = curr_len2;

	/* And make sure that we have enough room */
	if (len > sizeof(state->public_buffer))
		len = sizeof(state->public_buffer);

	Platform::Array<unsigned char>^ arrByte = ref new Platform::Array<unsigned char>(len);
	if (arrByte == nullptr)
		return 0;
	local_reader->ReadBytes(arrByte);

	memcpy(buf, arrByte->Data, len);
	local_reader->DetachStream();

	stm->rp = buf;
	stm->wp = buf + len;
	stm->pos += len;
	if (len == 0)
		return EOF;
	return *stm->rp++;
}

static void win_seek_file(fz_context *ctx, fz_stream *stm, int64_t offset, int whence)
{
	void *temp = (void*) stm->state;
	win_stream_struct *stream = reinterpret_cast <win_stream_struct*> (temp);
	IRandomAccessStream^ Stream = stream->stream;
	unsigned long long curr_pos = Stream->Position;
	unsigned long long length = Stream->Size;
	unsigned long long n;

	if (whence == SEEK_END)
	{
		n = length + offset;
	}
	else if (whence == SEEK_CUR)
	{
		n = curr_pos + offset;
	}
	else if (whence == SEEK_SET)
	{
		n = offset;
	}
	Stream->Seek(n);
	curr_pos = Stream->Position;
	stm->pos = n;
	stm->wp = stm->rp;
}

static void win_close_file(fz_context *ctx, void *state)
{
	win_stream_struct *win_stream = reinterpret_cast <win_stream_struct*> (state);
	IRandomAccessStream^ stream = win_stream->stream;
	delete stream;
}

status_t muctx::InitializeStream(IRandomAccessStream^ readStream, char *ext)
{
	win_stream.stream = readStream;
	void *temp = reinterpret_cast <void*> (&win_stream);
	fz_stream *mu_stream = fz_new_stream(mu_ctx, temp, win_next_file, win_close_file);
	mu_stream->seek = win_seek_file;

	/* Now lets see if we can open the file */
	fz_try(mu_ctx)
	{
		mu_doc = fz_open_document_with_stream(mu_ctx, ext, mu_stream);
	}
	fz_always(mu_ctx)
	{
		fz_drop_stream(mu_ctx, mu_stream);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}
#else
status_t muctx::OpenDocument(char *filename)
{
	pdf_document *idoc;

	fz_try(mu_ctx)
	{
		this->mu_doc = fz_open_document(mu_ctx, filename);
		/* Enable js if we are a pdf document */
		idoc = pdf_specifics(mu_ctx, mu_doc);
		if (idoc)
		{
			pdf_enable_js(mu_ctx, idoc);
			//pdf_set_doc_event_callback(mu_ctx, idoc, event_cb, app);
		}

	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}
#endif

/* mutext functions see mupdf readme for details */
static void lock_mutex(void *user, int lock)
{
	LPCRITICAL_SECTION locks = (LPCRITICAL_SECTION)user;
	EnterCriticalSection(&locks[lock]);
}

static void unlock_mutex(void *user, int lock)
{
	LPCRITICAL_SECTION locks = (LPCRITICAL_SECTION)user;
	LeaveCriticalSection(&locks[lock]);
}

/* Tries to get page from cache, if not there add it to the cache */
fz_page* muctx::LoadPage(int page_num)
{
	fz_page *page = NULL;
	int width, height;

	/* If object found, Use will have incremented the rc */
	page = (fz_page*) page_cache->Use(page_num, &width, &height, mu_ctx);
	if (page == NULL)
	{
		fz_var(page);

		fz_try(mu_ctx)
		{
			page = fz_load_page(mu_ctx, mu_doc, page_num);
			page_cache->Add(page_num, 0, 0, (void*)page, mu_ctx);
		}
		fz_catch(mu_ctx)
		{
			return NULL;
		}
	}
	/* The caller should drop the page when it is done */
	return page;
}

void muctx::CleanUp(void)
{
	display_cache->Empty(mu_ctx);
	annot_display_cache->Empty(mu_ctx);
	page_cache->Empty(mu_ctx);

	delete display_cache;
	delete annot_display_cache;
	delete page_cache;
	annot_display_cache = NULL;
	display_cache = NULL;
	page_cache = NULL;
}

void muctx::SetAA(int level)
{
	fz_set_aa_level(mu_ctx, level);
}

/* Set up the context, mutex and cookie */
status_t muctx::InitializeContext()
{
	int i;

	/* Get the mutexes set up */
	for (i = 0; i < FZ_LOCK_MAX; i++)
		InitializeCriticalSectionEx(&mu_criticalsec[i], 0, 0);
	mu_locks.user = &mu_criticalsec[0];
	mu_locks.lock = lock_mutex;
	mu_locks.unlock = unlock_mutex;

	/* Allocate the context */
	this->mu_ctx = fz_new_context(NULL, &mu_locks, FZ_STORE_DEFAULT);
	if (this->mu_ctx == NULL)
	{
		return E_OUTOFMEM;
	}
	else
	{
		fz_register_document_handlers(this->mu_ctx);
		return S_ISOK;
	}
}

/* Initializer */
muctx::muctx(void)
{
	mu_ctx = NULL;
	mu_doc = NULL;
	mu_outline = NULL;
	display_cache = new Cache();
	display_cache->CacheInit(DISPLAY_LIST);
	annot_display_cache = new Cache();
	annot_display_cache->CacheInit(DISPLAY_LIST);
	page_cache = new Cache();
	page_cache->CacheInit(PAGE);
}

/* Destructor */
muctx::~muctx(void)
{
	fz_drop_outline(mu_ctx, mu_outline);
	fz_drop_document(mu_ctx, mu_doc);
	fz_drop_context(mu_ctx);

	for (int i = 0; i < FZ_LOCK_MAX; i++)
		DeleteCriticalSection(&(mu_criticalsec[i]));

	mu_doc = NULL;
	mu_outline = NULL;
}

/* Return the documents page count */
int muctx::GetPageCount()
{
	int num_pages = 0;

	if (mu_doc == NULL)
		return -1;
	else
	{
		fz_try(mu_ctx)
		{
			num_pages = fz_count_pages(mu_ctx, mu_doc);
		}
		fz_catch(mu_ctx)
		{
			return E_FAILURE;
		}
		return num_pages;
	}
}

/* Get page size.  Do not use page cache for this.  This is used maninly to
   get the sizes for all the pages during initialization */
int muctx::MeasurePage(int page_num, point_t *size)
{
	fz_rect rect;
	fz_page *page = NULL;
	fz_rect bounds;

	fz_var(page);
	fz_var(bounds);

	fz_try(mu_ctx)
	{
		page = fz_load_page(mu_ctx, mu_doc, page_num);
		bounds = fz_bound_page(mu_ctx, page);
		size->X = bounds.x1 - bounds.x0;
		size->Y = bounds.y1 - bounds.y0;
	}
	fz_always(mu_ctx)
	{
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return 0;
}

/* Get page size */
point_t muctx::MeasurePage(fz_page *page)
{
	point_t pageSize;
	fz_rect rect;
	fz_rect bounds;

	bounds = fz_bound_page(mu_ctx, page);
	pageSize.X = bounds.x1 - bounds.x0;
	pageSize.Y = bounds.y1 - bounds.y0;

	return pageSize;
}

void muctx::FlattenOutline(fz_outline *outline, int level,
			  sh_vector_content contents_vec)
{
	char indent[8*4+1];
	float xp, yp;
	if (level > 8)
		level = 8;
	memset(indent, ' ', level * 4);
	indent[level * 4] = 0;

	std::string indent_str = indent;
	std::string str_indent;

	while (outline)
	{
		if (!fz_is_external_link(mu_ctx, outline->uri))
		{
			fz_location page = fz_resolve_link(mu_ctx, mu_doc, outline->uri, &xp, &yp);
			if (page.page >= 0 && outline->title)
			{
				/* Add to the contents std:vec */
				sh_content content_item(new content_t());
				content_item->page = page.page;
				content_item->string_orig = outline->title;
				str_indent = content_item->string_orig;
				str_indent.insert(0, indent_str);
				content_item->string_margin = str_indent;
				contents_vec->push_back(content_item);
			}
		}
		FlattenOutline(outline->down, level + 1, contents_vec);
		outline = outline->next;
	}
}

int muctx::GetContents(sh_vector_content contents_vec)
{
	fz_outline *root = NULL;
	int has_content = 0;

	fz_var(root);

	fz_try(mu_ctx)
	{
		root = fz_load_outline(mu_ctx, mu_doc);
		if (root != NULL)
		{
			has_content = 1;
			FlattenOutline(root, 0, contents_vec);
		}
	}
	fz_always(mu_ctx)
	{
		fz_drop_outline(mu_ctx, root);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return has_content;
}

int muctx::GetTextSearch(int page_num, char* needle, sh_vector_text texts_vec)
{
	fz_page *page = NULL;
	fz_device *dev = NULL;
	fz_stext_page *text = NULL;

	int hit_count = 0;
	int k;

	fz_var(page);
	fz_var(dev);
	fz_var(text);

	fz_try(mu_ctx)
	{
		text = fz_new_stext_page(mu_ctx, fz_bound_page(mu_ctx, page));
		dev = fz_new_stext_device(mu_ctx, text, 0);
		page = LoadPage(page_num);
		fz_run_page(mu_ctx, page, dev, fz_identity, NULL);
		fz_close_device(mu_ctx, dev);
		fz_drop_device(mu_ctx, dev);
		dev = NULL;
		fz_quad quad;
		hit_count = fz_search_stext_page(mu_ctx, text, needle, &quad, 1);

		for (k = 0; k < hit_count; k++)
		{
			sh_text text_search(new text_search_t());
			text_search->upper_left.X = mu_hit_bbox[k].x0;
			text_search->upper_left.Y = mu_hit_bbox[k].y0;
			text_search->lower_right.X = mu_hit_bbox[k].x1;
			text_search->lower_right.Y = mu_hit_bbox[k].y1;
			texts_vec->push_back(text_search);
		}
	}
	fz_always(mu_ctx)
	{
		fz_drop_page(mu_ctx, page);
		fz_drop_device(mu_ctx, dev);
		fz_drop_stext_page(mu_ctx, text);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return hit_count;
}

/* Due to a widget change, we want to update the page, and remove the
	annotation display list from the cache */
int muctx::WidgetChange(int page_num)
{
	fz_page *page = NULL;

	fz_var(page);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);
		pdf_update_page(mu_ctx, (pdf_page*)page);
		annot_display_cache->Drop(page_num, mu_ctx);
	}
	fz_always(mu_ctx)
	{
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return 0;
}


/* Get the widget information for the page and pack into a smart pointer */
unsigned int muctx::GetWidgets(int page_num, sh_vector_widget widgets_vec)
{
	fz_page *page = NULL;
	pdf_widget *widgets = NULL;
	pdf_document *idoc;
	int k = 0;
	unsigned int num_widgets = 0;

	fz_var(idoc);
	fz_var(widgets);
	fz_var(page);

	fz_try(mu_ctx)
	{
		idoc = pdf_specifics(mu_ctx, mu_doc); /* This just does a casting with null return if not PDF */
		if (idoc != NULL)
		{
			page = LoadPage(page_num);
			widgets = pdf_first_widget(mu_ctx, (pdf_page *)page);

			pdf_widget *curr_widget = widgets;
			if (curr_widget != NULL)
			{
				/* Get our smart pointer structure filled */
				while (curr_widget != NULL)
				{
					fz_rect rect;
					pdf_bound_widget(mu_ctx, curr_widget);

					sh_widget widget(new document_widget_t());

					widget->upper_left.X = rect.x0;
					widget->upper_left.Y = rect.y0;
					widget->lower_right.X = rect.x1;
					widget->lower_right.Y = rect.y1;
					widget->type = pdf_widget_type(mu_ctx, curr_widget);

					widgets_vec->push_back(widget);
					curr_widget = pdf_next_widget(mu_ctx, curr_widget);
					num_widgets += 1;
				}
			}
		}
	}
	fz_always(mu_ctx)
	{
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return num_widgets;
}

/* Get the annotation information for the page and pack into a smart pointer */
unsigned int muctx::GetAnnotations(int page_num, sh_vector_annot annotations_vec)
{
	fz_page *page = NULL;
	pdf_annot *annotation = NULL;
	pdf_obj *irt;
	pdf_document *idoc;
	int k = 0;
	unsigned int num_annots = 0;
	size_t lenstr;
	const char *str;

	fz_var(idoc);
	fz_var(annotation);
	fz_var(page);

	fz_try(mu_ctx)
	{
		idoc = pdf_specifics(mu_ctx, mu_doc); /* This just does a casting with null return if not PDF */
		if (idoc != NULL)
		{
			page = LoadPage(page_num);
			annotation = pdf_first_annot(mu_ctx, (pdf_page *)page);

			pdf_annot *curr_annot = annotation;
			if (curr_annot != NULL)
			{
				/* Get our smart pointer structure filled */
				while (curr_annot != NULL)
				{
					fz_rect rect;
					pdf_bound_annot(mu_ctx, curr_annot);
					/* Ignore PDF_ANNOT_WIDGET types as those are handled by
					 * a different loop */
					if (pdf_annot_type(mu_ctx, curr_annot) != PDF_ANNOT_WIDGET)
					{
						sh_annot annot(new document_annot_t());

						annot->upper_left.X = rect.x0;
						annot->upper_left.Y = rect.y0;
						annot->lower_right.X = rect.x1;
						annot->lower_right.Y = rect.y1;
						annot->type = pdf_annot_type(mu_ctx, curr_annot);

						/* Get the ref number for this annotation.  This is
						* is needed so that we can properly use the IRT (in reply to)
						* field */
						annot->num = pdf_to_num(mu_ctx, curr_annot->obj);

						irt = 0;// pdf_annot_irt(mu_ctx, curr_annot);
						if (irt != NULL)
							annot->rt_num = pdf_to_num(mu_ctx, irt);
						else
							annot->rt_num = 0;

						/* The strings */
						str = pdf_annot_author(mu_ctx, curr_annot);
						if (str != NULL)
						{
							lenstr = strlen(str);
							std::unique_ptr<char[]> val(new char[lenstr + 1]);
							strcpy_s(val.get(), lenstr + 1, str);
							annot->author.swap(val); /* annot in charge of deletion */
						}
						else
						{
							annot->author = NULL;
						}
						str = NULL; // pdf_annot_date(mu_ctx, curr_annot);
						if (str != NULL)
						{
							lenstr = strlen(str);
							std::unique_ptr<char[]> val(new char[lenstr + 1]);
							strcpy_s(val.get(), lenstr + 1, str);
							annot->date.swap(val); /* annot in charge of deletion */
						}
						else
						{
							annot->date = NULL;
						}
						str = pdf_annot_contents(mu_ctx, curr_annot);
						if (str != NULL)
						{
							lenstr = strlen(str);
							std::unique_ptr<char[]> val(new char[lenstr + 1]);
							strcpy_s(val.get(), lenstr + 1, str);
							annot->contents.swap(val); /* annot in charge of deletion */
						}
						else
						{
							annot->contents = NULL;
						}
						annotations_vec->push_back(annot);
						num_annots += 1;
					}
					curr_annot = pdf_next_annot(mu_ctx, curr_annot);
				}
			}
		}
	}
	fz_always(mu_ctx)
	{
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return num_annots;
}

/* Get the links and pack into a smart pointer structure */
unsigned int muctx::GetLinks(int page_num, sh_vector_link links_vec)
{
	fz_page *page = NULL;
	fz_link *links = NULL;
	int k = 0;
	unsigned int num_links = 0;
	float xp, yp;

	fz_var(page);
	fz_var(links);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);
		links = fz_load_links(mu_ctx, page);

		fz_link *curr_link = links;
		if (curr_link != NULL)
		{
			/* Get our smart pointer structure filled */
			while (curr_link != NULL)
			{
				fz_rect curr_rect = curr_link->rect;
				sh_link link(new document_link_t());

				link->upper_left.X = curr_rect.x0;
				link->upper_left.Y = curr_rect.y0;
				link->lower_right.X = curr_rect.x1;
				link->lower_right.Y = curr_rect.y1;

				if (fz_is_external_link(mu_ctx, curr_link->uri))
				{
					size_t lenstr = strlen(curr_link->uri);
					std::unique_ptr<char[]> uri(new char[lenstr + 1]);
					strcpy_s(uri.get(), lenstr + 1, curr_link->uri);
					link->uri.swap(uri);
					link->type = LINK_URI;
				}
				else
				{
					link->type = LINK_GOTO;
					auto location = fz_resolve_link(mu_ctx, mu_doc,
						curr_link->uri, &xp, &yp);

					link->page_num = location.page;

				}
				links_vec->push_back(link);
				curr_link = curr_link->next;
				num_links += 1;
			}
		}
	}
	fz_always(mu_ctx)
	{
		fz_drop_page(mu_ctx, page);
		fz_drop_link(mu_ctx, links);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return num_links;
}

fz_display_list * muctx::CreateAnnotationList(int page_num)
{
	fz_device *dev = NULL;
	fz_page *page = NULL;
	int width, height;

	/* First see if we have this one in the cache */
	fz_display_list *dlist =
		(fz_display_list*) annot_display_cache->Use(page_num, &width, &height, mu_ctx);
	if (dlist != NULL)
		return dlist;

	/* Apparently not, lets go ahead and create and add to cache */
	fz_var(dev);
	fz_var(page);
	fz_var(dlist);

	fz_try(mu_ctx)
	{
		pdf_annot *annot;
		page = LoadPage(page_num);
		annot = pdf_first_annot(mu_ctx, (pdf_page*)page);
		if (annot != NULL)
		{
			fz_rect bounds;
			/* Create display list */
			dlist = fz_new_display_list(mu_ctx, bounds);
			dev = fz_new_list_device(mu_ctx, dlist);

			for (annot = pdf_first_annot(mu_ctx, (pdf_page*)page); annot; annot = pdf_next_annot(mu_ctx, annot))
				pdf_run_annot(mu_ctx, annot, dev, fz_identity, NULL);
			annot_display_cache->Add(page_num, 0, 0, (void*) dlist, mu_ctx);
		}
	}
	fz_always(mu_ctx)
	{
		fz_drop_device(mu_ctx, dev);
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		fz_drop_display_list(mu_ctx, dlist);
		return NULL;
	}
	return dlist;
}

fz_display_list * muctx::CreateDisplayList(int page_num, int *width, int *height)
{
	fz_device *dev = NULL;
	fz_page *page = NULL;
	point_t page_size;

	/* First see if we have this one in the cache */
	fz_display_list *dlist =
		(fz_display_list*) display_cache->Use(page_num, width, height, mu_ctx);
	if (dlist != NULL)
		return dlist;

	/* Apparently not, lets go ahead and create and add to cache */
	fz_var(dev);
	fz_var(page);
	fz_var(dlist);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);

		/* Create a new list */
		fz_rect bounds;
		dlist = fz_new_display_list(mu_ctx, bounds);
		dev = fz_new_list_device(mu_ctx, dlist);
		fz_run_page_contents(mu_ctx, page, dev, fz_identity, NULL);
		page_size = MeasurePage(page);
		*width = page_size.X;
		*height = page_size.Y;
		/* Add it to the cache and set that it is in use */
		display_cache->Add(page_num, *width, *height, (void*) dlist, mu_ctx);
	}
	fz_always(mu_ctx)
	{
		fz_drop_device(mu_ctx, dev);
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		fz_drop_display_list(mu_ctx, dlist);
		return NULL;
	}
	return dlist;
}

void muctx::ReleaseDisplayLists(void *opdlist, void *opannotlist)
{
	fz_display_list *dlist = (fz_display_list*) opdlist;
	fz_display_list *annotlist = (fz_display_list*) opannotlist;

	if (dlist != NULL)
	{
		fz_drop_display_list(mu_ctx, dlist);
	}
	if (annotlist != NULL)
	{
		fz_drop_display_list(mu_ctx, annotlist);
	}
}

/* A special version which will create the display list AND get the information
   that we need for various text selection tasks */
fz_display_list * muctx::CreateDisplayListText(int page_num, int *width, int *height,
	fz_stext_page **text_out, int *length)
{
	fz_stext_page *text = NULL;  /* Returned so not dropped here */
	fz_device *dev = NULL;
	fz_device *textdev = NULL;
	fz_page *page = NULL;
	fz_rect mediabox;

	point_t page_size;
	*length = 0;

	/* First see if we have this one in the cache */
	fz_display_list *dlist =
		(fz_display_list*) display_cache->Use(page_num, width, height, mu_ctx);
	if (dlist != NULL)
		return dlist;

	/* Apparently not, lets go ahead and create and add to cache, getting
	   the page's text information at the same time. */
	fz_var(dev);
	fz_var(textdev);
	fz_var(page);
	fz_var(dlist);
	fz_var(text);

	fz_try(mu_ctx)
	{
		fz_rect bounds;
		page = LoadPage(page_num);
		text = fz_new_stext_page(mu_ctx, fz_bound_page(mu_ctx, page));

		/* Create a new list */
		dlist = fz_new_display_list(mu_ctx, bounds);
		dev = fz_new_list_device(mu_ctx, dlist);

		/* Deal with text device */
		textdev = fz_new_stext_device(mu_ctx, text, 0);
		fz_run_page(mu_ctx, page, textdev, fz_identity, NULL);

		fz_close_device(mu_ctx, textdev);
		*text_out = text;
		fz_drop_device(mu_ctx, textdev);
		textdev = NULL;

		fz_run_page_contents(mu_ctx, page, dev, fz_identity, NULL);
		page_size = MeasurePage(page);
		*width = page_size.X;
		*height = page_size.Y;
		/* Add it to the cache and set that it is in use */
		display_cache->Add(page_num, *width, *height, (void*) dlist, mu_ctx);
	}
	fz_always(mu_ctx)
	{
		fz_drop_device(mu_ctx, textdev);
		fz_drop_device(mu_ctx, dev);
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		fz_drop_display_list(mu_ctx, dlist);
		return NULL;
	}
	return dlist;
}

/* Render display list bmp_data buffer.  No lock needed for this operation */
status_t muctx::RenderPageMT(void *dlist, void *a_dlist, int page_height,
							 unsigned char *bmp_data, int bmp_width, int bmp_height,
							 float scale, bool flipy, point_t top_left)
{
	fz_device *dev = NULL;
	fz_pixmap *pix = NULL;
	fz_matrix ctm, *pctm = &ctm;
	fz_context *ctx_clone = NULL;
	fz_display_list *display_list = (fz_display_list*) dlist;
	fz_display_list *annot_displaylist = (fz_display_list*) a_dlist;

	ctx_clone = fz_clone_context(mu_ctx);

	fz_var(dev);
	fz_var(pix);
	fz_var(display_list);
	fz_var(annot_displaylist);

	fz_try(ctx_clone)
	{
		//TODO: scalling issues
		//pctm = fz_scale(pctm, scale, scale);
		/* Flip on Y. */
		if (flipy)
		{
			ctm.f = (float) page_height * ctm.d;
			ctm.d = -ctm.d;
			ctm.f += top_left.Y;
		}
		else
		{
			ctm.f -= top_left.Y;
		}
		ctm.e -= top_left.X;
		pix = fz_new_pixmap_with_data(ctx_clone, fz_device_bgr(ctx_clone),
			bmp_width, bmp_height, NULL, 1, bmp_width * 4, bmp_data);
		fz_clear_pixmap_with_value(ctx_clone, pix, 255);
		dev = fz_new_draw_device(ctx_clone, ctm, pix);
		fz_rect sessior1;
		fz_rect sessior2;
		fz_run_display_list(ctx_clone, display_list, dev, fz_identity, sessior1, NULL);
		if (annot_displaylist != NULL)
			fz_run_display_list(ctx_clone, annot_displaylist, dev, fz_identity, sessior2, NULL);
	}
	fz_always(ctx_clone)
	{
		fz_drop_device(ctx_clone, dev);
		fz_drop_pixmap(ctx_clone, pix);
	}
	fz_catch(ctx_clone)
	{
		fz_drop_context(ctx_clone);
		return E_FAILURE;
	}
	fz_drop_context(ctx_clone);
	return S_ISOK;
}

/* Render page_num to size width by height into bmp_data buffer.  Lock needed. */
status_t muctx::RenderPage(int page_num, unsigned char *bmp_data, int bmp_width,
						   int bmp_height, float scale, bool flipy)
{
	fz_device *dev = NULL;
	fz_pixmap *pix = NULL;
	fz_page *page = NULL;
	fz_matrix ctm, *pctm = &ctm;
	point_t page_size;

	fz_var(dev);
	fz_var(pix);
	fz_var(page);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);
		page_size = MeasurePage(page);
		//TODO:scale issue
		//pctm = fz_scale(pctm, scale, scale);
		/* Flip on Y */
		if (flipy)
		{
			ctm.f = bmp_height;
			ctm.d = -ctm.d;
		}
		pix = fz_new_pixmap_with_data(mu_ctx, fz_device_bgr(mu_ctx), bmp_width,
										bmp_height,NULL, 1, bmp_width * 4, bmp_data);
		fz_clear_pixmap_with_value(mu_ctx, pix, 255);
		dev = fz_new_draw_device(mu_ctx, ctm, pix);
		fz_run_page_contents(mu_ctx, page, dev, fz_identity, NULL);

		pdf_annot *annot;
		for (annot = pdf_first_annot(mu_ctx, (pdf_page*)page); annot; annot = pdf_next_annot(mu_ctx, annot))
			pdf_run_annot(mu_ctx, annot, dev, fz_identity, NULL);
	}
	fz_always(mu_ctx)
	{
		fz_drop_device(mu_ctx, dev);
		fz_drop_pixmap(mu_ctx, pix);
		fz_drop_page(mu_ctx, page);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}

bool muctx::RequiresPassword(void)
{
	return fz_needs_password(mu_ctx, mu_doc) != 0;
}

bool muctx::ApplyPassword(char* password)
{
	return fz_authenticate_password(mu_ctx, mu_doc, password) != 0;
}

std::string muctx::GetText(int page_num, int type)
{
	fz_output *out = NULL;
	fz_device *dev = NULL;
	fz_page *page = NULL;
	fz_stext_page *text = NULL;
	fz_buffer *buf = NULL;
	std::string output;
	fz_rect mediabox;

	fz_var(out);
	fz_var(dev);
	fz_var(page);
	fz_var(text);
	fz_var(buf);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);
		text = fz_new_stext_page(mu_ctx, fz_bound_page(mu_ctx, page));
		dev = fz_new_stext_device(mu_ctx, text, 0);
		fz_run_page(mu_ctx, page, dev, fz_identity, NULL);
		fz_close_device(mu_ctx, dev);
		fz_drop_device(mu_ctx, dev);
		dev = NULL;
		//TODO:  output text issue
		//fz_analyze_text(mu_ctx, text);
		//buf = fz_new_buffer(mu_ctx, 256);
		//out = fz_new_output_with_buffer(mu_ctx, buf);
		//if (type == HTML)
		//	fz_print_stext_page_html(mu_ctx, out, text);
		//else if (type == XML)
		//	fz_print_stext_page_xml(mu_ctx, out, text);
		//else
		//	fz_print_stext_page(mu_ctx, out, text);
		output = std::string(((const char*) fz_string_from_buffer(mu_ctx, buf)));
	}
	fz_always(mu_ctx)
	{
		fz_drop_output(mu_ctx, out);
		fz_drop_device(mu_ctx, dev);
		fz_drop_page(mu_ctx, page);
		fz_drop_stext_page(mu_ctx, text);
		fz_drop_buffer(mu_ctx, buf);
	}
	fz_catch(mu_ctx)
	{
		return nullptr;
	}
	return output;
}

void muctx::ReleaseText(void *text)
{
	fz_stext_page *text_page = (fz_stext_page*) text;
	fz_drop_stext_page(mu_ctx, text_page);
}

/* To do: banding */
status_t muctx::SavePage(char *filename, int page_num, int resolution, int type,
	bool append)
{
	float zoom;
	fz_matrix ctm;
	fz_rect bounds, tbounds;
	fz_output *out = NULL;
	fz_device *dev = NULL;
	int width, height;
	fz_display_list *dlist = NULL;
	fz_display_list *annot_dlist = NULL;
	fz_page *page = NULL;
	bool valid = true;
	fz_pixmap *pix = NULL;
	fz_irect ibounds;

	fz_var(dev);
	fz_var(page);
	fz_var(dlist);
	fz_var(annot_dlist);
	fz_var(pix);
	fz_var(out);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);
		bounds = fz_bound_page(mu_ctx, page);
		zoom = resolution / 72;
		//TODO: scale issue
		//fz_scale(&ctm, zoom, zoom);
		tbounds = bounds;
		fz_transform_rect(tbounds, ctm);
		ibounds = fz_round_rect(tbounds);

		/* First see if we have this one in the cache */
		dlist = (fz_display_list*) display_cache->Use(page_num, &width, &height, mu_ctx);
		annot_dlist = (fz_display_list*) annot_display_cache->Use(page_num, &width, &height, mu_ctx);

		if (type == SVG_OUT)
		{
			out = fz_new_output_with_path(mu_ctx, filename, 0);

			dev = fz_new_svg_device(mu_ctx, out, tbounds.x1 - tbounds.x0, tbounds.y1 - tbounds.y0, FZ_SVG_TEXT_AS_PATH, true);
			if (dlist != NULL)
				fz_run_display_list(mu_ctx, dlist, dev, ctm, tbounds, NULL);
			else
				fz_run_page(mu_ctx, page, dev, ctm, NULL);
			if (annot_dlist != NULL)
				fz_run_display_list(mu_ctx, annot_dlist, dev, ctm, tbounds, NULL);
			else
			{
				pdf_annot *annot;
				for (annot = pdf_first_annot(mu_ctx, (pdf_page*) page); annot; annot = pdf_next_annot(mu_ctx, annot))
					pdf_run_annot(mu_ctx, annot, dev, fz_identity, NULL);
			}
		}
		else
		{
			pix = fz_new_pixmap_with_bbox(mu_ctx, fz_device_rgb(mu_ctx), ibounds,NULL, 1);
			fz_set_pixmap_resolution(mu_ctx, pix, resolution, resolution);
			fz_clear_pixmap_with_value(mu_ctx, pix, 255);
			dev = fz_new_draw_device(mu_ctx, ctm, pix);
			if (dlist != NULL)
				fz_run_display_list(mu_ctx, dlist, dev, fz_identity, tbounds, NULL);
			else
				fz_run_page(mu_ctx, page, dev, fz_identity, NULL);
			if (annot_dlist != NULL)
				fz_run_display_list(mu_ctx, annot_dlist, dev, fz_identity, tbounds, NULL);
			else
			{
				pdf_annot *annot;
				for (annot = pdf_first_annot(mu_ctx,(pdf_page*) page); annot; annot = pdf_next_annot(mu_ctx, annot))
					pdf_run_annot(mu_ctx, annot, dev, fz_identity, NULL);
			}
			switch (type)
			{
				case PNM_OUT:
					fz_save_pixmap_as_pnm(mu_ctx, pix, filename);
					break;
				case PCL_OUT: /* This can do multi-page */
					fz_pcl_options options;
					fz_pcl_preset(mu_ctx, &options, "ljet4");
					fz_save_pixmap_as_pcl(mu_ctx, pix, filename, append, &options);
					break;
				case PWG_OUT: /* This can do multi-page */
					fz_save_pixmap_as_pwg(mu_ctx, pix, filename, append, NULL);
					break;
			}
		}
	}
	fz_always(mu_ctx)
	{
		if (pix != NULL)
			fz_drop_pixmap(mu_ctx, pix);
		fz_drop_device(mu_ctx, dev);
		fz_drop_page(mu_ctx, page);
		if (out != NULL)
		{
			fz_drop_output(mu_ctx, out);
		}
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}

status_t muctx::PDFClean(char *infile, char *outfile, char *password,
	pdf_write_options *options, char *argv[], int argc)
{
	fz_try(mu_ctx)
	{
		pdf_clean_file(mu_ctx, infile, outfile, password, options, argv, argc);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}

/* Todo add print and display profile to mupdf code */
status_t muctx::CreateProof(const char *pdf_file, const char *filename, int res,
	const char *print_profile, const char *display_profile)
{
	fz_try(mu_ctx)
	{
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}

/* Separation related calls */
int muctx::GetNumSeparations(void *page_in)
{
	fz_page *page = (fz_page*) page_in;
	int num_seps = 0;

	fz_var(page);

	fz_try(mu_ctx)
	{
		num_seps = fz_count_separations(mu_ctx, NULL);
	}
	fz_catch(mu_ctx)
	{
		return 0;
	}
	return num_seps;
}

const char* muctx::GetSeparationInfo(void *page_in, int sep_num, uint32_t *rgba, uint32_t *cmyk)
{
	fz_page *page = (fz_page*)page_in;
	const char *name = NULL;

	fz_var(page);

	fz_try(mu_ctx)
	{
		//TODO: seperation issue
		name = NULL; // fz_get_separation_on_page(mu_ctx, page, sep_num, rgba, cmyk);
	}
	fz_catch(mu_ctx)
	{
		return NULL;
	}
	return name;
}

void muctx::ControlSeparation(void *page_in, int sep_num, int disable)
{
	fz_page *page = (fz_page*)page_in;

	fz_var(page);

	fz_try(mu_ctx)
	{
		//TODO: contol seperation issue
		//fz_control_separation_on_page(mu_ctx, page, sep_num, disable);
	}
	fz_catch(mu_ctx)
	{
		return;
	}
	return;
}

/* Rerender a page.  Used for proofing where we have the page and have changed
   a setting.  E.g. the separations to have active */
status_t muctx::ReRenderPage(void *page_in, unsigned char *bmp_data, int bmp_width,
	int bmp_height, float scale, bool flipy)
{
	fz_device *dev = NULL;
	fz_pixmap *pix = NULL;
	fz_page *page = (fz_page*)page_in;
	fz_matrix ctm, *pctm = &ctm;
	point_t page_size;

	fz_var(dev);
	fz_var(pix);
	fz_var(page);

	fz_try(mu_ctx)
	{
		page_size = MeasurePage(page);
		//TODO: scale issue
		//pctm = fz_scale(pctm, scale, scale);
		/* Flip on Y */
		if (flipy)
		{
			ctm.f = bmp_height;
			ctm.d = -ctm.d;
		}
		pix = fz_new_pixmap_with_data(mu_ctx, fz_device_bgr(mu_ctx), bmp_width,
			bmp_height,NULL, 1, bmp_width * 4, bmp_data);
		fz_clear_pixmap_with_value(mu_ctx, pix, 255);
		dev = fz_new_draw_device(mu_ctx, ctm, pix);
		fz_run_page_contents(mu_ctx, page, dev, fz_identity, NULL);
	}
	fz_always(mu_ctx)
	{
		fz_drop_device(mu_ctx, dev);
		fz_drop_pixmap(mu_ctx, pix);
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return S_ISOK;
}

/* Render page_num to size width by height into bmp_data buffer.  Lock needed.
   Also, note that the page is returned. */
void* muctx::RenderPageSave(int page_num, unsigned char *bmp_data, int bmp_width,
	int bmp_height, float scale, bool flipy)
{
	fz_device *dev = NULL;
	fz_pixmap *pix = NULL;
	fz_page *page = NULL;
	fz_matrix ctm, *pctm = &ctm;
	point_t page_size;

	fz_var(dev);
	fz_var(pix);
	fz_var(page);

	fz_try(mu_ctx)
	{
		page = LoadPage(page_num);
		page_size = MeasurePage(page);
		//TODO: scale issue
		//pctm = fz_scale(pctm, scale, scale);
		/* Flip on Y */
		if (flipy)
		{
			ctm.f = bmp_height;
			ctm.d = -ctm.d;
		}
		pix = fz_new_pixmap_with_data(mu_ctx, fz_device_bgr(mu_ctx), bmp_width,
			bmp_height,NULL, 1, bmp_width * 4, bmp_data);
		fz_clear_pixmap_with_value(mu_ctx, pix, 255);
		dev = fz_new_draw_device(mu_ctx, ctm, pix);
		fz_run_page_contents(mu_ctx, page, dev, fz_identity, NULL);
	}
	fz_always(mu_ctx)
	{
		fz_drop_device(mu_ctx, dev);
		fz_drop_pixmap(mu_ctx, pix);
	}
	fz_catch(mu_ctx)
	{
		return NULL;
	}
	return (void*) page;
}

void muctx::ReleasePage(void *page_in)
{
	fz_page *page = (fz_page*)page_in;
	fz_drop_page(mu_ctx, page);
}

int muctx::IncrementalSaveFile(char *filename)
{
	pdf_write_options opts;
	pdf_document *idoc;

	opts.do_incremental = 1;
	opts.do_ascii = 0;
	opts.do_decompress = 0;
	opts.do_garbage = 0;
	opts.do_linear = 0;

	fz_try(mu_ctx)
	{
		idoc = pdf_specifics(mu_ctx, mu_doc);
		if (idoc != NULL)
		{
			pdf_save_document(mu_ctx, idoc, filename, &opts);
		}
	}
	fz_catch(mu_ctx)
	{
		return E_FAILURE;
	}
	return 0;
}

/* Annotation support.  Get the information for the annotation on the page */

