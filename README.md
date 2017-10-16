# BuildDependency

Provides a cross-platform compatible msbuild task named "dependencies" that allows to download
artifacts from a TeamCity server, as well as a GUI to edit the dependencies.

## What does it do?

TeamCity allows to define dependencies as part of a build job definition. However, that helps
only on the TeamCity server itself, not when trying to build on a developer's machine or on a
non-TeamCity build server.

Chris Hubbard's [BuildUpdate](https://github.com/chrisvire/BuildUpdate) tool improves the
situation in that it can read the TeamCity job definition and create an update script that
allows to download the artifacts from the TeamCity server.

While this tool improves things it still has some drawbacks:

- it still requires to define the dependencies in TeamCity first and as a second step to create
  the update script and commit that. This can lead to broken builds if the new dependencies
  won't work with the old code, but the new code can't be committed without having the new
  dependencies available.
- it downloads the files sequentially, i.e. the next download will start when the previous has
  finished.
- multiple platforms and architectures require separate scripts if the dependencies differ.

__BuildDependency__ uses a slightly different approach. While it is possible (in the
BuildDependencyManager GUI) to select the artifacts that a TeamCity build produces, the
dependencies itself are defined locally in a git-config-style `*.dep` file. Each dependency can
specify the platform and architecture it applies to.

The downloads happen on multiple threads. A full download will occur only if the file on the
server is newer than the existing local file. Additionally files are cached, so if multiple
projects have overlapping dependencies the file will be copied from the cache which can save a
download. Caching the files also allows to build when the Internet connection or the TeamCity
server are not available.

The `*.dep` dependency file can be edited by the BuildDependencyManager GUI app, or directly
in a text editor.


## Installation

### Build task

Install the [BuildDependencyTasks](https://www.nuget.org/packages/BuildDependencyTasks) nuget
package, either in the solution (for use with a msbuild `*.proj` file), or directly in a project.

### BuildDependencyManager GUI

Download, unpack and run the latest `BuildDependencyManager.zip` file from
[GitHub](https://github.com/ermshiperete/BuildDependency/releases).
