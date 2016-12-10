using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AnyBar.CLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var parser = new Parser(s => {
                s.CaseInsensitiveEnumValues = true;
            });

            var exitCode = parser
                .ParseArguments<Options>(args)
                .MapResult(
                    options => {
                        Console.WriteLine($"[{options.Host}:{options.Port} => {options.Color}]");
                        IPAddress ipAddress;
                        AnyBarClient client;
                        if(IPAddress.TryParse(options.Host, out ipAddress))
                            client = new AnyBarClient(ipAddress, options.Port);
                        else
                            client = new AnyBarClient(options.Host, options.Port);
                        
                        client.Change(options.Color);
                        return 0;
                    },
                    errors =>
                    {
                        var colorError = (errors as IEnumerable<Error>)
                            .FirstOrDefault(e => e is BadFormatConversionError)
                            as BadFormatConversionError;
                        if (colorError?.NameInfo.LongName == "color")
                            Console.WriteLine("Color can be one of: {0}", String.Join(", ", Enum.GetNames(typeof(AnyBarImage))));
                        return 1;
                    });
            return exitCode;
        }
    }
}
