# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

[comment]: <> (Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
)

## [Unreleased]

### Fixed

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
