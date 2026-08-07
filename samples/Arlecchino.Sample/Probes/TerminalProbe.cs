using System;
using System.Text;
using System.Threading;

namespace Arlecchino.Sample.Probes;

internal static class TerminalProbe
{
    public static void Ask(int milliseconds)
    {
        Console.WriteLine($"Asking, and listening for {milliseconds} ms. Do not press anything.");

        Console.Out.Write(
            "\e_Gi=31,s=1,v=1,a=q,t=d,f=24;AAAA\e\\" +
            "\e[16t" +
            "\e[14t" +
            "\e]11;?\a" +
            "\e[c");

        Console.Out.Flush();

        var heard = new StringBuilder();
        var until = DateTime.UtcNow.AddMilliseconds(milliseconds);

        while (DateTime.UtcNow < until)
        {
            if (Console.KeyAvailable)
            {
                heard.Append(Console.ReadKey(true).KeyChar);
                continue;
            }

            Thread.Sleep(1);
        }

        Console.WriteLine();
        Console.WriteLine($"Heard {heard.Length} characters:");
        Console.WriteLine(heard.ToString()
            .Replace("\e", "<ESC>", StringComparison.Ordinal)
            .Replace("\a", "<BEL>", StringComparison.Ordinal));
    }
}
