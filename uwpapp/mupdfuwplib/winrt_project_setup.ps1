# Windows power shell script to copy the project files to the winRT types 
# User should first open the visual studio solution for mupdf with VS 2015

cd ./libmupdf/platform/win32/

# Copy first, and then setup libmupdf for using gs.  libmupdf_winrt does not
# need gproof etc.

if(Test-Path libmupdf_orig.vcxproj) {
	Copy-Item libmupdf_orig.vcxproj libmupdf.vcxproj 
	Copy-Item libmupdf_orig.vcxproj.filters libmupdf.vcxproj.filters 
}
else { 
	Copy-Item libmupdf.vcxproj libmupdf_orig.vcxproj
	Copy-Item libmupdf.vcxproj.filters libmupdf_orig.vcxproj.filters
}

if(Test-Path libthirdparty_orig.vcxproj) {
	Copy-Item libthirdparty_orig.vcxproj libthirdparty.vcxproj 
	Copy-Item libthirdparty_orig.vcxproj.filters libthirdparty.vcxproj.filters 
}
else { 
	Copy-Item libthirdparty.vcxproj libthirdparty_orig.vcxproj
	Copy-Item libthirdparty.vcxproj.filters libthirdparty_orig.vcxproj.filters
}

<# 
#Now after the copy, setup libmupdf for using gs with the gproof device and the api.
(gc libmupdf.vcxproj) -replace 
"<PreprocessorDefinitions>", 
"<PreprocessorDefinitions>FZ_ENABLE_GPRF;GSVIEW_WIN;HAVE_IAPI_H" | 
Out-File libmupdf.vcxproj -encoding utf8

#Also we need the path to iapi.h for invoking gs from mupdf
(gc libmupdf.vcxproj) -replace 
"<AdditionalIncludeDirectories>", 
"<AdditionalIncludeDirectories>..\..\..\ghostpdl\psi;" | 
Out-File libmupdf.vcxproj -encoding utf8

#Also need to look at adding USE_OUTPUT_DEBUG_STRING for debug configs

#And finally we need the gsdll64.lib or gsdll32.lib debug or release
#This requires a bit of work since we are adding lines and have to
#make sure we are in the right place
$c = (gc libmupdf.vcxproj)
$just_past_compile = $False
$in_compile = $False
$x64 = $False
$debug = $False
$ouput = @() #empty array
$lf = "`r`n"
foreach ($line in $c)
{
# Determine which build we are in and make sure we are just outside the compile options
    $test_compile_in = select-string -pattern '<ClCompile>' -InputObject $line
    $test_compile_out = select-string -pattern '</ClCompile>' -InputObject $line
    $test_debug = select-string -pattern 'DEBUG=1' -InputObject $line
    $test_win32 = select-string -pattern 'Win32' -InputObject $line
    $test_x64 = select-string -pattern 'x64' -InputObject $line

    if ($test_compile_in.length -ne 0)
    {
      $just_past_compile = $False
      $in_compile = $True
    }
    if ($test_compile_out.length -ne 0)
    {
      $just_past_compile = $True
      $in_compile = $False
    }
    if ($test_win32.length -ne 0)
    {
      $x64 = $False
    }
    if ($test_x64.length -ne 0)
    {
      $x64 = $True
    }
    
    if ($in_compile)
    {
      if ($test_debug.length -ne 0)
      {
        $debug = $True
      }    
    }
    
    if ($just_past_compile) 
    {
      $output = $output + $line
      $output = $output + $lf
      #now add the new content
      $output = $output + '    <Lib>'
      $output = $output + $lf
      if ($debug)
      {
        if ($x64)
        {
          $output = $output + '      <AdditionalDependencies>../../../ghostpdl/debugbin/gsdll64.lib</AdditionalDependencies>'   
        }
        else
        {
          $output = $output + '      <AdditionalDependencies>../../../ghostpdl/debugbin/gsdll32.lib</AdditionalDependencies>'   
        }     
      }
      else
      {
        if ($x64)
        {
          $output = $output + '      <AdditionalDependencies>../../../ghostpdl/bin/gsdll64.lib</AdditionalDependencies>'   
        }
        else
        {
          $output = $output + '      <AdditionalDependencies>../../../ghostpdl/bin/gsdll32.lib</AdditionalDependencies>'   
        }     
      }
      $output = $output + $lf
      $output = $output + '    </Lib>'
      $output = $output + $lf
      $just_past_compile = $False
      $debug = $False
    } 
    else
    {
      $output = $output + $line
      $output = $output + $lf
    }
}
$output | Out-File libmupdf.vcxproj -encoding utf8
$output = $null

 #>
 
# Now back to the winRT projects.
# Replace the run time libraries.
(gc libmupdf.vcxproj) -replace 
"<RuntimeLibrary>MultiThreadedDebug</RuntimeLibrary>", 
"<RuntimeLibrary>MultiThreadedDebugDLL</RuntimeLibrary>" | 
Out-File libmupdf.vcxproj -encoding utf8

(gc libthirdparty.vcxproj) -replace 
"<RuntimeLibrary>MultiThreadedDebug</RuntimeLibrary>", 
"<RuntimeLibrary>MultiThreadedDebugDLL</RuntimeLibrary>" | 
Out-File libthirdparty.vcxproj -encoding utf8

(gc libmupdf.vcxproj) -replace 
"<RuntimeLibrary>MultiThreaded</RuntimeLibrary>", 
"<RuntimeLibrary>MultiThreadedDLL</RuntimeLibrary>" | 
Out-File libmupdf.vcxproj -encoding utf8

(gc libthirdparty.vcxproj) -replace 
"<RuntimeLibrary>MultiThreaded</RuntimeLibrary>", 
"<RuntimeLibrary>MultiThreadedDLL</RuntimeLibrary>" | 
Out-File libthirdparty.vcxproj -encoding utf8

# Use v120 platform toolset (VS 2013 version) replace uses regex
# looks for start (?<=<PlatformToolset>), stuff between .*?, and end (?=<) (part of </PlatformToolset>)
# where the stuff between gets replaced with v120
(gc libmupdf.vcxproj) -replace 
"(?<=<PlatformToolset>).*?(?=<)", 
"v142" | 
Out-File libmupdf.vcxproj -encoding utf8

(gc libthirdparty.vcxproj) -replace 
"(?<=<PlatformToolset>).*?(?=<)", 
"v142" | 
Out-File libthirdparty.vcxproj -encoding utf8

# Make Windows 8.1 type project
(gc libmupdf.vcxproj) -replace 
"<CharacterSet>MultiByte</CharacterSet>", 
"<CharacterSet>MultiByte</CharacterSet>`r`n    <WindowsAppContainer>true</WindowsAppContainer>" | 
Out-File libmupdf.vcxproj -encoding utf8

(gc libthirdparty.vcxproj) -replace 
"<CharacterSet>MultiByte</CharacterSet>", 
"<CharacterSet>MultiByte</CharacterSet>`r`n    <WindowsAppContainer>true</WindowsAppContainer>" | 
Out-File libthirdparty.vcxproj -encoding utf8

# Add NO_GETENV to libthirdparty for openjpeg memory management and WinRT
# Note openjpeg repos recently broken as it still has a setenv
(gc libthirdparty.vcxproj) -replace 
"<PreprocessorDefinitions>", "<PreprocessorDefinitions>NO_GETENV;" | 
Out-File libthirdparty.vcxproj -encoding utf8

# Add _WINRT to libmupdf due to an issue with the 
# use of GetSystemTimeAsFileTime in time.c of the fitz library
# debug config is easy as it already has preprocessor defines
(gc libmupdf.vcxproj) -replace "<PreprocessorDefinitions>", 
"<PreprocessorDefinitions>_WINRT;" | Out-File libmupdf.vcxproj -encoding utf8

# release config is an issue since it does not have any preprocessor defines
# in this case we have to do this programmatically
$c = (gc libmupdf.vcxproj)
$compile = $False
$release = $False
$ouput = @() #empty array
$lf = "`r`n"
foreach ($line in $c)
{
# Determine which build we are in and make sure we are in the compile options
    $test_compile_in = select-string -pattern '<ClCompile>' -InputObject $line
    $test_compile_out = select-string -pattern '</ClCompile>' -InputObject $line
    $test_release = select-string -pattern 'Release' -InputObject $line
    $test_debug = select-string -pattern 'Debug' -InputObject $line
   
    if ($test_compile_in.length -ne 0)
    {
      $compile = $True
    }
    if ($test_compile_out.length -ne 0)
    {
      $compile = $False
    }
    if ($test_release.length -ne 0)
    {
      $release = $True
    }
    if ($test_debug.length -ne 0)
    {
      $release = $False
    }
    
    if ($release -and $compile)
    {
      $output = $output + $line
      $output = $output + $lf
      #now add the options
      $output = $output + '      <PreprocessorDefinitions>_WINRT;%(PreprocessorDefinitions)</PreprocessorDefinitions>'    
      $output = $output + $lf
      $compile = $False #only allow one case of this 
    }
    else
    {
      $output = $output + $line
      $output = $output + $lf
    }     
}
$output | Out-File libmupdf.vcxproj -encoding utf8
$output = $null

$r='</AdditionalIncludeDirectories>`r`n      <RuntimeLibrary>'
$v='</AdditionalIncludeDirectories>`r`n    <PreprocessorDefinitions>_WINRT;%(PreprocessorDefinitions)</PreprocessorDefinitions>`r`n      <RuntimeLibrary>'
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8

# Set project names
#(gc libmupdf.vcxproj) -replace 
#"<PropertyGroup Label=""Globals"">", 
#"<PropertyGroup Label=""Globals"">`r`n    <ProjectName>libmupdf</ProjectName>" | 
#Out-File libmupdf.vcxproj -encoding utf8

#(gc libthirdparty.vcxproj) -replace 
#"<PropertyGroup Label=""Globals"">", 
#"<PropertyGroup Label=""Globals"">`r`n    <ProjectName>libthirdparty</ProjectName>" | 
#Out-File libthirdparty.vcxproj -encoding utf8

# Add information about Min Visual Studio Version and about app store
(gc libmupdf.vcxproj) -replace "<RootNamespace>mupdf</RootNamespace>", 
"<RootNamespace>mupdf</RootNamespace>`r`n    <MinimumVisualStudioVersion>16.0</MinimumVisualStudioVersion>`r`n    <ApplicationType>Windows Store</ApplicationType>`r`n    <ApplicationTypeRevision>10.0</ApplicationTypeRevision>`r`n    <WindowsTargetPlatformVersion>10.0</WindowsTargetPlatformVersion>" | 
Out-File libmupdf.vcxproj -encoding utf8

(gc libthirdparty.vcxproj) -replace "<RootNamespace>mupdf</RootNamespace>", 
"<RootNamespace>mupdf</RootNamespace>`r`n    <MinimumVisualStudioVersion>16.0</MinimumVisualStudioVersion>`r`n    <ApplicationType>Windows Store</ApplicationType>`r`n    <ApplicationTypeRevision>10.0</ApplicationTypeRevision>`r`n    <WindowsTargetPlatformVersion>10.0</WindowsTargetPlatformVersion>" | 
Out-File libthirdparty.vcxproj -encoding utf8

# Not using precompiled headers and set warning level
(gc libmupdf.vcxproj) -replace 
"</ClCompile>", 
"  <PrecompiledHeader>NotUsing</PrecompiledHeader>`r`n    <SDLCheck>false</SDLCheck>`r`n</ClCompile>" | 
Out-File libmupdf.vcxproj -encoding utf8

(gc libthirdparty.vcxproj) -replace 
"</ClCompile>", 
"  <PrecompiledHeader>NotUsing</PrecompiledHeader>`r`n    <SDLCheck>false</SDLCheck>`r`n</ClCompile>" | 
Out-File libthirdparty.vcxproj -encoding utf8

# Set path names for output and intermediate directory. This syntax is required to handle
# use of () in the strings
#thirdparty
$r='<OutDir>$(Configuration)\</OutDir>'
$v='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
(gc libthirdparty.vcxproj).Replace($r,$v) | Out-File libthirdparty.vcxproj -encoding utf8

$r='<IntDir>$(Configuration)\$(ProjectName)\</IntDir>'
$v='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
(gc libthirdparty.vcxproj).Replace($r,$v) | Out-File libthirdparty.vcxproj -encoding utf8

$r='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
$v='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
(gc libthirdparty.vcxproj).Replace($r,$v) | Out-File libthirdparty.vcxproj -encoding utf8

$r='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
$v='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
(gc libthirdparty.vcxproj).Replace($r,$v) | Out-File libthirdparty.vcxproj -encoding utf8

#libmupdf
$r='<OutDir>$(Configuration)\</OutDir>'
$v='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8

$r='<IntDir>$(Configuration)\$(ProjectName)\</IntDir>'
$v='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8

$r='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
$v='<OutDir>$(Platform)\$(Configuration)\</OutDir>'
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8

$r='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
$v='<IntDir>$(Platform)\$(Configuration)\$(ProjectName)\</IntDir>'
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8

# Set character set 
$r='<CharacterSet>MultiByte</CharacterSet>'
$v='<CharacterSet>UniCode</CharacterSet>'
(gc libthirdparty.vcxproj).Replace($r,$v) | Out-File libthirdparty.vcxproj -encoding utf8
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8

# Remove prebuild event.  This is handled with project dependency and causes an error for the
# winRT build
$r='Generate CMap and Font source files'
$v=''
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8
$r='generate.bat'
$v=''
(gc libmupdf.vcxproj).Replace($r,$v) | Out-File libmupdf.vcxproj -encoding utf8
