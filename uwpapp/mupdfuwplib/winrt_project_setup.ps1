# Windows power shell script to copy the project files to the winRT types 
# User should first open the visual studio solution for mupdf with VS 2015

cd ./libmupdf/platform/win32/

# Copy first, and then setup libmupdf for using gs.  libmupdf_winrt does not
# need gproof etc.

$filenames = @('libmupdf','libthirdparty','libluratech','libpkcs7','libresources') 

# Now back to the winRT projects.
# Replace the run time libraries.

foreach ($item in $filenames) {

  $filename = $($item + ".vcxproj")
  Write-Host "Output: $filename"

  if(Test-Path $($item + "_orig.vcxproj")) {
    Copy-Item $($item + "_orig.vcxproj") $($item + ".vcxproj")
    Copy-Item $($item + "_orig.vcxproj.filters") $($item + ".vcxproj.filters")
  }
  else { 
    Copy-Item $($item + ".vcxproj") $($item + "_orig.vcxproj")
    Copy-Item $($item +".vcxproj.filters") $($item + "_orig.vcxproj.filters")
  }

  $r='<RuntimeLibrary>MultiThreadedDebug</RuntimeLibrary>'
  $v='<RuntimeLibrary>MultiThreadedDebugDLL</RuntimeLibrary>'
  (gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

  $r='<RuntimeLibrary>MultiThreaded</RuntimeLibrary>'
  $v='<RuntimeLibrary>MultiThreadedDLL</RuntimeLibrary>'
  (gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

  # Use v120 platform toolset (VS 2013 version) replace uses regex
  # looks for start (?<=<PlatformToolset>), stuff between .*?, and end (?=<) (part of </PlatformToolset>)
  # where the stuff between gets replaced with v120
  (gc $filename) -replace 
  "(?<=<PlatformToolset>).*?(?=<)", 
  "v142" | 
  Out-File $filename -encoding utf8

  # Make Windows 8.1 type project and Set character set 
  (gc $filename) -replace 
  "<CharacterSet>MultiByte</CharacterSet>", 
  "<CharacterSet>UniCode</CharacterSet>`r`n    <WindowsAppContainer>true</WindowsAppContainer>`r`n    <UseDebugLibraries>false</UseDebugLibraries>" | 
  Out-File $filename -encoding utf8

  # Add _WINRT to libmupdf due to an issue with the 
  # use of GetSystemTimeAsFileTime in time.c of the fitz library
  # debug config is easy as it already has preprocessor defines
  (gc $filename) -replace
  "<PreprocessorDefinitions>", 
  "<PreprocessorDefinitions>_WINRT;" |
  Out-File $filename -encoding utf8

  # Set project names
  #(gc libmupdf.vcxproj) -replace 
  #"<PropertyGroup Label=""Globals"">", 
  #"<PropertyGroup Label=""Globals"">`r`n    <ProjectName>libmupdf</ProjectName>" | 
  #Out-File libmupdf.vcxproj -encoding utf8

  # Add information about Min Visual Studio Version and about app store
  (gc $filename) -replace
  '<RootNamespace>(.+)</RootNamespace>', 
  $('<RootNamespace>$1</RootNamespace>' + "`r`n    <MinimumVisualStudioVersion>14.0</MinimumVisualStudioVersion>`r`n    <ApplicationType>Windows Store</ApplicationType>`r`n    <ApplicationTypeRevision>10.0</ApplicationTypeRevision>`r`n    <WindowsTargetPlatformVersion>10.0</WindowsTargetPlatformVersion>") | 
  Out-File $filename -encoding utf8

  # Not using precompiled headers and set warning level
  (gc $filename) -replace 
  "</ClCompile>", 
  "  <PrecompiledHeader>NotUsing</PrecompiledHeader>`r`n      <SDLCheck>false</SDLCheck>`r`n      <CompileAsWinRT>false</CompileAsWinRT>`r`n</ClCompile>" | 
  Out-File $filename -encoding utf8


  # Set path names for output and intermediate directory. This syntax is required to handle
  # use of () in the strings
  #thirdparty
  $r='<OutDir>$(Configuration)\</OutDir>'
  $v='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
  (gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

  $r='<IntDir>$(Configuration)\$(ProjectName)\</IntDir>'
  $v='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
  (gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

  $r='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
  $v='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
  (gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

  $r='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
  $v='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
  (gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

}

$filename = "bin2coff.vcxproj"
$r='<OutDir>$(SolutionDir)$(Configuration)\</OutDir>'
$v='<OutDir>$(Configuration)\</OutDir>'
(gc $filename).Replace($r,$v) | Out-File $filename -encoding utf8

# Add NO_GETENV to libthirdparty for openjpeg memory management and WinRT
# Note openjpeg repos recently broken as it still has a setenv
$filename = "libthirdparty.vcxproj"
(gc $filename) -replace 
"<PreprocessorDefinitions>",
"<PreprocessorDefinitions>NO_GETENV;HB_NO_MMAP;" | 
Out-File $filename -encoding utf8

# Remove prebuild event.  This is handled with project dependency and causes an error for the
# winRT build
$filename = "libmupdf.vcxproj"
# (gc $filename) -replace 
# "<PreprocessorDefinitions>",
# "<PreprocessorDefinitions>TOFU;" | 
# Out-File $filename -encoding utf8

(gc $filename) -replace 
'Generate CMap and Font source files',
'' |
Out-File $filename -encoding utf8

(gc $filename) -replace 
'generate.bat',
'' |
Out-File $filename -encoding utf8

