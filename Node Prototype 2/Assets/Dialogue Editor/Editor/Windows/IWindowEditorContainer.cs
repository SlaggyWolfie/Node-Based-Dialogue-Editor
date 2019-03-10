using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WolfEditor.Editor
{
    public interface IWindowEditorContainer
    {
        WindowEditor Editor { get; }
    }
}
