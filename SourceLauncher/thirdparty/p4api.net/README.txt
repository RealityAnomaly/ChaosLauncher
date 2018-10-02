
                     Perforce .NET API Binary Distribution

===============================================================================

Directory Structure:

p4api.net                         
|- doc                            Usage documentation
|- examples                       Sample applications
|  |- bin                         Files generated during sample app. build
|  |  |- Debug                    Sample app. debug build files
|  |  |- Release                  Sample app. release build files
|  |- sln-bld-cmd                 Command line sample app.
|  |- sln-bld-gui                 Graphical sample app.
|- lib                            Files required for building an .NET API app.

===============================================================================

Using the .NET API:

1) In Visual Studio 2010 create a new project.

2) Build your newly created project with the "Debug" configuration.

3) Copy the files from the .NET API "lib" directory into the "bin\Debug" 
   directory of the project you just created.

3) In your Visual Studio project add a reference to the
   "bin\Debug\p4api.net.dll" file in your project's directory.

4) Add Perforce .NET API functionality to your project by referencing the
   usage documentation.

NOTES: 

Both "p4api.net.dll" and "p4bridge.dll" must be present in the application's
directory for the application to run.

===============================================================================

Building the Example Applications:

1) Open the example applications solution with Visual Studio 2010.

2) Confirm that the build platform is set to the correct configuration 
   ("x86" or "x64").

3) Build the example applications by right clicking on the solution and 
   selecting "Build Solution" from the menu.

4) The newly built applications will be in the "Debug" or "Release" 
   subdirectory of the "examples\bin" directory depending on the selected 
   configuration.

NOTES:

sln-bld-cmd.exe is a console application that builds a solution from a Perforce
depot. For usage run "sln-bld-cmd.exe /?". Builds are made in a directory named 
with a timestamp below the current working directory.

sln-bld-gui.exe is a Windows form application that builds a solution from a 
Perforce depot. Host, port, user, depot path of the solution, target build
directory, and location of "MSBuild.exe" are all required. Builds are made in a 
directory named with a timestamp below the specified target directory. Once a 
depot path is defined the application will check for changes submitted to that
location in the depot on a build interval which can be defined by the dropdown 
control. The default is 2 minutes.

Both applications creates a temporary workspace named 
"p4apinet_solution_builder_sample_application_client" and delete this workspace
on completion of the sync of files to the local machine. The sync command 
forces resynchronization and does not update the server's knowledge of the file
sync state. 

Both "p4api.net.dll" and "p4bridge.dll" must be present in the applications'
directory for the applications to run.

This product includes software developed by the OpenSSL Project for
use in the OpenSSL Toolkit (http://www.openssl.org/)
This product includes software and cryptographic software written by
Eric Young (eay@cryptsoft.com).
This product includes software written by Tim Hudson (tjh@cryptsoft.com).
http://www.openssl.org/source/license.html 

===============================================================================

For changes between releases, please see the release notes: p4api.netnotes.txt
which can be found at www.perforce.com