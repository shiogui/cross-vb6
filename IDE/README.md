# Development Plan

Our development plan and guide for the VB6 IDE is ready.

### _Summary of the Path Forward_

1.  _Start in C# with SDL3-CS:_ This allows you to build the complex logic of the _Piece Table_ (text buffer) and _AST_ (compiler) without worrying about manual memory management immediately.
2.  _The "Single Source of Truth":_ Ensure your compiler and editor share the same buffer. When the user types, the Lexer should update the highlighting tokens instantly.
3.  _The "TCC" Shortcut:_ Don't write machine code (x86/x64) yourself. Transpile your VB6 code into a simple C file and have the _Tiny C Compiler_ compile it. It is extremely small and can be bundled with your IDE.
4.  _The Form Designer:_ Use _Dear ImGui_ for the properties panel. It will save you months of work building text boxes, sliders, and color pickers for your UI.

### _Initial Implementation Checklist_

- [ ] _Window:_ Init SDL3 and a black background.
- [ ] _Text:_ Load a font and render a single line from a char[] array.
- [ ] _Buffer:_ Implement the Gap Buffer so you can type and delete.
- [ ] _Lexer:_ Highlight the word Sub in blue and String in light blue.
- [ ] _Canvas:_ Draw a grey rectangle that represents the VB6 Form.
      By following this modular approach, your move to _C_ will be a matter of swapping syntax rather than redesigning the logic. Since you are targeting _VB6_, you have the advantage of a stable, well-documented language target—allowing you to focus entirely on making the IDE the fastest one ever built for it.
