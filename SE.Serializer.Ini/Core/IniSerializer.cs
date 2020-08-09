using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Core
{
    public static class IniSerializer
    {
        // How the ini serializer will work:
        //     - Entries are grouped into sections. Entries are pretty much an entry within a Dictionary<string, string>.
        //
        // THE PARSER:
        //     - Set current section to default / null.
        //     - Loop through a string separated by newline. For each line:
        //         - IF:      Starts with comment char '#', then add the line to a list called NextElementComments.
        //         - ELSE IF: Starts with the section start char '[', then set the current section to whatever is within the brackets.
        //         - ELSE:    Create an entry for the current section, where:
        //                    KEY   = Left side of the separator char '='.
        //                    VALUE = Right side of the separator char.
        // 
        // THE SERIALIZER:
        //     - The serializer is used to serialize custom classes from and to INI.
        //     - For reading, works after the parser has done it's work.
        //     - Use attributes to separate sections within custom classes.
        //     TODO: Plan.

        // PARSER ATTRIBUTES EXAMPLE:
        //
        // public class Example
        // {
        //     [IniSection(Name: "Screen", Comments: "Screen settings.")]
        //     public int ScreenSizeX = 1920;
        //     public int ScreenSizeY = 1080;
        //
        //     [IniSection(Name: "User", Comments: "User settings.")]
        //     public string Name = "Bob";
        // }
    }
}
