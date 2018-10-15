# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

## [0.4.3] - 2018-10-15

### Changed

- now compiled against .NET Framework 4.6.2

### Fixed

- add default values for RevisionName and RevisionValue

## [0.4.2] - 2018-04-11

### Changed

- Updated Bugsnag library and exception handling to v2 API

## [0.4.1] - 2017-10-20

### Fixed

- Resuming and caching of incomplete downloads (#28)

## [0.4.0] - 2017-10-19

### Added

- Add _Platform_ property to task to allow to specify target platform

### Fixed

- Add support for Windows 64-bit to task (#27)

## [0.3.1] - 2017-10-17

### Fixed

- overwrite existing file when copying file from cache

## [0.3.0] - 2017-10-16

### Added

- Implement caching of dependencies (#16)
- Implement offline mode (#17)
- Add support for Windows 64-bit in UI (#25) - thanks @vkarthim

### Changed

- Improve error message if server can't be reached

### Fixed

- Don't ignore last line of dependency file
- Treat servers that differ only in protocol or port as identical

## [0.2.11] - 2017-10-06

### Changed

- Improve error message if TC project name changed (#19)

[comment]: <> (Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
)
