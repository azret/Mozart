using System;
using System.Ai;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

static partial class App {
    static bool LearnSpellingVariations(Session session,
            string spell,
            Func<bool> IsTerminated) {
        if (spell.StartsWith("--spell")) {
            spell = spell.Remove(0, "--spell".Length).Trim();
        } else if (spell.StartsWith("spell")) {
            spell = spell.Remove(0, "spell".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        LearnSpellingVariations(
            session.Model,
            MakeFiles(new string[] { session.CurrentDirectory },
                session.SearchPattern, SearchOption.AllDirectories),
            session.Lex);
        Model.SaveToFile(
            Matrix.Sort(session.Model),
            "CBOW",
            CBOW.DIMS,
            Path.ChangeExtension(session.OutputFileName, ".okurrr"));
        return false;
    }

    static void LearnSpellingVariations(Matrix Model, Set files, IOrthography lex) {
        Console.Write($"Learning spelling variations...\r\n");
        // void ZeroOutVariations() {
        //     foreach (var it in Model) {
        //         it.Clear();
        //     }
        // }
        // ZeroOutVariations();
        foreach (string file in (IEnumerable<string>)files) {
            Console.Write($"Reading {file}...\r\n");
            // string textFragment = File.ReadAllText(file);
            // foreach (var t in PlainText.ForEach(textFragment, 0, textFragment.Length, 0)) {
            //     if (t.Type == PlainTextTag.TAG) {
            //         var s = t.TextFragment.Substring(t.StartIndex, t.Length);
            //         var it = Model[lex.GetKey(s)];
            //         if (it != null) {
            //             it.Push(s,
            //                 out Scalar spellingVariation);
            //             if (s.Length > 1 && char.IsLetter(s[0])
            //                     && char.IsUpper(s[0]) && !char.IsUpper(s[1])) {
            //                         /* Give more weight to capitalized words... */
            //                 spellingVariation.Add(2d / CBOW.THRESHOLD);
            //             } else {
            //                 spellingVariation.Add(1d / CBOW.THRESHOLD);
            //             }
            //         }
            //     }
            // }
        }
        Console.Write($"\r\nReady!\r\n");
    }
}