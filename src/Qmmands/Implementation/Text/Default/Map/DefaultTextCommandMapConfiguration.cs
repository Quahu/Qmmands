using System;

namespace Qmmands.Text.Default;

public class DefaultTextCommandMapConfiguration
{
    public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    public string Separator { get; set; } = " ";
}
