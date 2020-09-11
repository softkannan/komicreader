#pragma once

#include <memory>
#include <functional>
#include <vector>
#include <windows.h>
#include "status.h"
#include "Cache.h"
extern "C" {
#include "mupdf/fitz.h"
#include "mupdf/pdf.h"
#include "mupdf/pdf/event.h"
}


#define MAX_SEARCH 500
/* 25% .. 1600% */
#define MINRES 18
#define MAXRES 1152

enum { SVG_OUT, PNM_OUT, PCL_OUT, PWG_OUT };
enum { HTML = 0, XML, TEXT };
enum { CONTENT_LIST = 0, CONTENT_EXPORT, CONTENT_VAL };
enum { SIG_OBTAINED = 0, SIG_NOTSAVED, SIG_NOTSIGNED, SIG_NOTSIGWIDGET, SIG_GOTSIZES, SIG_ERROR };
enum { ARROW, HAND, WAIT, CARET };
enum { DISCARD, SAVE, CANCEL };
enum { QUERY_NO, QUERY_YES };

struct pdfapp_s
{
	/* current document params */
	fz_document* doc;
	char* docpath;
	char* doctitle;
	fz_outline* outline;
	int outline_deferred;

	float layout_w;
	float layout_h;
	float layout_em;
	char* layout_css;
	int layout_use_doc_css;

	int pagecount;

	/* current view params */
	float default_resolution;
	float resolution;
	int rotate;
	fz_pixmap* image;
	int imgw, imgh;
	int grayscale;
	fz_colorspace* colorspace;
	int invert;
	int tint, tint_white;
	int useicc;
	int useseparations;
	int aalevel;

	/* presentation mode */
	int presentation_mode;
	int transitions_enabled;
	fz_pixmap* old_image;
	fz_pixmap* new_image;
	clock_t start_time;
	int in_transit;
	float duration;
	fz_transition transition;

	/* current page params */
	int pageno;
	fz_page* page;
	fz_rect page_bbox;
	fz_display_list* page_list;
	fz_display_list* annotations_list;
	fz_stext_page* page_text;
	fz_link* page_links;
	int errored;
	int incomplete;

	/* separations */
	fz_separations* seps;

	/* snapback history */
	int hist[256];
	int histlen;
	int marks[10];

	/* window system sizes */
	int winw, winh;
	int scrw, scrh;
	int shrinkwrap;
	int fullscreen;

	/* event handling state */
	char number[256];
	int numberlen;

	int ispanning;
	int panx, pany;

	int iscopying;
	int selx, sely;
	/* TODO - While sely keeps track of the relative change in
	 * cursor position between two ticks/events, beyondy shall keep
	 * track of the relative change in cursor position from the
	 * point where the user hits a scrolling limit. This is ugly.
	 * Used in pdfapp.c:pdfapp_onmouse.
	 */
	int beyondy;
	fz_rect selr;

	int nowaitcursor;

	/* search state */
	int issearching;
	int searchdir;
	char search[512];
	int searchpage;
	fz_quad hit_bbox[512];
	int hit_count;

	/* client context storage */
	void* userdata;

	fz_context* ctx;
#ifdef HAVE_CURL
	fz_stream* stream;
#endif
};

typedef struct pdfapp_s pdfapp_t;

typedef struct point_s
{
	double X;
	double Y;
} point_t;

/* Links */
typedef struct document_link_s
{
	link_t type;
	point_t upper_left;
	point_t lower_right;
	std::unique_ptr<char[]> uri;
	int page_num;
} document_link_t;
#define sh_link std::shared_ptr<document_link_t>
#define sh_vector_link std::shared_ptr<std::vector<sh_link>>

/* Text Search */
typedef struct text_search_s
{
	point_t upper_left;
	point_t lower_right;
} text_search_t;
#define sh_text std::shared_ptr<text_search_t>
#define sh_vector_text std::shared_ptr<std::vector<sh_text>>

/* Widgets */
typedef struct document_widget_s
{
	point_t upper_left;
	point_t lower_right;
	int type;
} document_widget_t;
#define sh_widget std::shared_ptr<document_widget_t>
#define sh_vector_widget std::shared_ptr<std::vector<sh_widget>>

/* Widget list items */
typedef struct widget_content_s
{
	std::unique_ptr<char[]> item_string;
} widget_content_t;
#define sh_widget_content std::shared_ptr<widget_content_t>
#define sh_vector_widget_content std::shared_ptr<std::vector<sh_widget_content>>

/* Annotations */
typedef struct document_annot_s
{
	point_t upper_left;
	point_t lower_right;
	int type;
	int num;
	int rt_num;
	std::unique_ptr<char[]> author;
	std::unique_ptr<char[]> contents;
	std::unique_ptr<char[]> date;
} document_annot_t;
#define sh_annot std::shared_ptr<document_annot_t>
#define sh_vector_annot std::shared_ptr<std::vector<sh_annot>>

/* Content Results */
typedef struct content_s
{
	int  page;
	std::string string_orig;
	std::string string_margin;
} content_t;
#define sh_content std::shared_ptr<content_t>
#define sh_vector_content std::shared_ptr<std::vector<sh_content>>

#ifdef _WINRT_DLL
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation;

typedef struct win_stream_struct_s
{
	IRandomAccessStream^ stream;
	unsigned char public_buffer[4096];
} win_stream_struct;
#else
typedef struct win_stream_struct_s
{
	char* stream;
} win_stream_struct;
#endif

class muctx
{
private:

	pdfapp_t* app;

	CRITICAL_SECTION mu_criticalsec[FZ_LOCK_MAX];
	win_stream_struct win_stream;
	fz_locks_context mu_locks;
	fz_context *mu_ctx;
	fz_document *mu_doc;
	fz_outline *mu_outline;
	fz_rect mu_hit_bbox[MAX_SEARCH];
	void FlattenOutline(fz_outline *outline, int level,
						sh_vector_content contents_vec);
	Cache *display_cache;
	Cache *annot_display_cache;
	Cache *page_cache;
	fz_page* LoadPage(int page_num);

public:
	muctx(void);
	~muctx(void);
	void CleanUp(void);
	int GetPageCount();

	status_t ContextInit();

	status_t RenderPage(int page_num, unsigned char *bmp_data, int bmp_width,
						int bmp_height, float scale, bool flipy);
	status_t RenderPageMT(void *dlist, void *a_dlist, int page_height,
							unsigned char *bmp_data, int bmp_width, int bmp_height,
							float scale, bool tile, point_t top_left);
	fz_display_list* CreateDisplayList(int page_num, int *width, int *height);
	fz_display_list * CreateDisplayListText(int page_num, int *width,
		int *height, fz_stext_page **text, int *length);
	fz_display_list * CreateAnnotationList(int page_num);
	void ReleaseDisplayLists(void *dlist, void *annotlist);
	int MeasurePage(int page_num, point_t *size);
	point_t MeasurePage(fz_page *page);
	unsigned int GetLinks(int page_num, sh_vector_link links_vec);
	unsigned int GetWidgets(int page_num, sh_vector_widget widgets_vec);
	unsigned int GetAnnotations(int page_num, sh_vector_annot annotations_vec);
	void SetAA(int level);
	int GetTextSearch(int page_num, char* needle, sh_vector_text texts_vec);
	int GetContents(sh_vector_content contents_vec);
	std::string GetText(int page_num, int type);
	void ReleaseText(void *text);
	bool RequiresPassword(void);
	bool ApplyPassword(char* password);
	status_t SavePage(char *filename, int pagenum, int resolution, int type,
		bool append);
	status_t PDFClean(char *infile, char *outfile, char *password,
		pdf_write_options *options, char *argv[], int argc);
	status_t CreateProof(const char *pdf_file, const char *filename, int res,
		const char *print_profile, const char *display_profile);
	int GetNumSeparations(void *page);
	const char* GetSeparationInfo(void *page, int sep_num, uint32_t *rgba,
		uint32_t *cmyk);
	void ControlSeparation(void *page, int sep_num, int disable);
	void* RenderPageSave(int page_num, unsigned char *bmp_data, int bmp_width,
		int bmp_height, float scale, bool flipy);
	status_t ReRenderPage(void *page_in, unsigned char *bmp_data, int bmp_width,
		int bmp_height, float scale, bool flipy);
	void ReleasePage(void *page);
	int WidgetChange(int page_num);
	int IncrementalSaveFile(char *filename);
	int GetSignature(int page_num, double x, double y, char *contents_ebuff,
		int *contents_len, int *byte_range_ebuff, int *byte_range_len);
#ifdef _WINRT_DLL
	status_t InitializeStream(IRandomAccessStream^ readStream, char *ext);
#else
	status_t OpenDocument(char *filename);
#endif

};
