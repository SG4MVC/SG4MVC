## SG4MVC Changelog
- **0.9.7-beta**
  - Fix for constructors issue
  - Add [NonController] to generated helper class
  - Performance: use compiled Regex in SanitiseFieldName

- **0.9.6-beta**
  - Upgrade to .NET 10
  - Primary constructor support in controllers
  - Upgrade Microsoft.CodeAnalysis.CSharp to 5.3.0
  - Use collection expressions and latest C# language features

- **0.9.5-beta**
  - Remove unused package reference

- **0.9.4-beta**
  - Prevent warnings about lowercase only class names

- **0.9.3-beta** Initial beta release, containing the core functionality
  - Basic project parsing, controller detection, rewrite and helper class generation 
  - Static link shortcuts from the `wwwroot` directory
  - Html helpers and Tag helpers for basic functionality (links, urls, forms)
  - Area support
