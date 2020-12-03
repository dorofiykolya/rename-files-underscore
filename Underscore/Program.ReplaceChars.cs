using System.Collections.Generic;

namespace Underscore
{
    partial class Program
    {
        private static readonly Dictionary<char, string> ReplaceMap = new Dictionary<char, string>
        {
            {'й', "iy"},
            {'ц', "c"},
            {'у', "y"},
            {'к', "k"},
            {'е', "e"},
            {'н', "n"},
            {'г', "g"},
            {'ш', "sh"},
            {'щ', "sch"},
            {'з', "e"},
            {'х', "h"},
            {'ї', "yi"},
            {'ф', "f"},
            {'і', "i"},
            {'в', "v"},
            {'а', "a"},
            {'п', "p"},
            {'р', "p"},
            {'о', "o"},
            {'л', "l"},
            {'д', "d"},
            {'ж', "j"},
            {'є', "e"},
            {'я', "ya"},
            {'ч', "ch"},
            {'с', "c"},
            {'м', "m"},
            {'и', "i"},
            {'т', "t"},
            {'ь', "b"},
            {'б', "b"},
            {'ю', "yu"},
            {'ъ', "_"},
            {'ы', "u"},
            {'ё', "yo"},
            {'э', "ue"},
        };
    }
}