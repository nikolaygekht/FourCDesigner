# File names

All files must have meaningful names that reflects the role of the file in the project.

# Encoding

All files must be in UTF-8 encoding.

# End-of-Line

  * Linux and Docker files (sh for example) must use linux style CR-only, not CR/LF line endings.

  * Code and documentation files: e.g. html, cs, js, css, md files must use windows style (CR/LF) line endings

# Project Structure

The project must have the following files and folders structure

The root folder of the project must consists of:

* `LICENSE.md` file that consists of the license
* `README.md` file that describes:
  * The purpose of the project
  * How to build and run the project
  * The structure of the project (how to navigate the project)
* bash script files to:
  * Build the project
  * Run the project

The project documentation must be located in the subfolder called `doc`

The user help must be located in the subfolder `help`. If the user help content is generated, then the source code of the help must be located in the folder `help-src`

The front-end part of the project must be located in the subfolder `www`. The scripts must be located in `www/scripts` and styles must be located in `www/styles`.

The back-end part of the project must be located in the subfolder `src`.

The docker files and scripts must be located in the subfolder `docker`.

# Language-Specific Coding Guides:

  * See [.NET Coding Guide](CODINGGUIDE_CS.md) for C# code
  * See [JS Coding Guide](CODINGGUIDE_JS.md) for JavaScript code
  * See [HTML/CSS Coding Guide](CODINGGUIDE_HTML.md) for HTML/JavaScript code

# Warnings

Warnings are BAD. FIX all warnings!!!