using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbigSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            SBIG.ActivateRelayParams arp = new SBIG.ActivateRelayParams();
            SBIG.OpenDeviceParams odp;

            try
            {
                // parse which device to open
                odp = new SBIG.OpenDeviceParams(args[0]);

                // parse duration input
                ushort duration = ushort.Parse(args[3]);

                // parse X direction
                switch (args[1][0])
                {
                    case '+':
                        arp.tXPlus = duration;
                        break;
                    case '-':
                        arp.tXMinus = duration;
                        break;
                    case '0':
                        break;
                    default:
                        throw new ArgumentException("X direction was none of +, -, or 0");
                }

                // parse Y direction
                switch (args[2][0])
                {
                    case '+':
                        arp.tYPlus = duration;
                        break;
                    case '-':
                        arp.tYMinus = duration;
                        break;
                    case '0':
                        break;
                    default:
                        throw new ArgumentException("Y direction was none of +, -, or 0");
                }
            }
            catch
            {
                Console.WriteLine("SbigRelayControl <IP|USB1-8> <+|-|0 for X> <+|-|0 for Y> <10s of ms to move|0 to stop>");
                return;
            }

            //
            // we've parsed all the input we need, so send the command!
            //
            try
            {
                // connect to the camera
                SBIG.UnivDrvCommand(SBIG.Cmd.CC_OPEN_DRIVER, null);
                SBIG.UnivDrvCommand(SBIG.Cmd.CC_OPEN_DEVICE, odp);
                SBIG.EstablishLink();

                //
                // send the command!
                //
                SBIG.UnivDrvCommand(SBIG.Cmd.CC_ACTIVATE_RELAY, arp);

                // disconnect
                SBIG.UnivDrvCommand(SBIG.Cmd.CC_CLOSE_DEVICE, null);
                SBIG.UnivDrvCommand(SBIG.Cmd.CC_CLOSE_DRIVER, null);

                Console.WriteLine("Success!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        } // Main

    } // class
} // namespace
