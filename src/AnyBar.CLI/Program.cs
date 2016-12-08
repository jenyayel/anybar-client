using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyBar.CLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            var exitCode = result
                .MapResult(
                    options => {
                        Console.WriteLine($"[{options.Host}:{options.Port} => {options.Color}]");
                        new AnyBarClient(options.Host, options.Port).Change(options.Color);
                        return 0;
                    },
                    errors =>
                    {
                        var colorError = (errors as IEnumerable<Error>)
                            .FirstOrDefault(e => e is BadFormatConversionError)
                            as BadFormatConversionError;
                        if (colorError.NameInfo.LongName == "color")
                            Console.WriteLine("Color can be one of: {0}", String.Join(", ", Enum.GetNames(typeof(AnyBarImage))));
                        return 1;
                    });
            return exitCode;
        }
    }
}
