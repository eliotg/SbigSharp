using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SbigSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // connect to the camera
            //
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_OPEN_DRIVER, null);

            // ask the SBIG driver what, if any, USB cameras are plugged in
            SBIG.QueryUsbResults qur = new SBIG.QueryUsbResults();
            SBIG.UnivDrvCommand_OutComplex(SBIG.Cmd.CC_QUERY_USB, null, qur);
            for (int i = 0; i < qur.camerasFound; i++)
            {
                if (!qur.dev[i].cameraFound)
                    Console.WriteLine("Cam " + i + ": not found");
                else
                    Console.WriteLine(String.Format("Cam {0}: type={1} name={2} ser={3}", i, qur.dev[i].cameraType, qur.dev[i].name, qur.dev[i].serialNumber));
            }

            SBIG.UnivDrvCommand(SBIG.Cmd.CC_OPEN_DEVICE, new SBIG.OpenDeviceParams("127.0.0.1"));
            SBIG.CameraType ct = SBIG.EstablishLink();


            // query temperature
            SBIG.QueryTemperatureStatusParams qtsp = new SBIG.QueryTemperatureStatusParams(SBIG.TempStatusRequest.TEMP_STATUS_ADVANCED2);
            var qtsr2 = SBIG.UnivDrvCommand<SBIG.QueryTemperatureStatusResults2>(SBIG.Cmd.CC_QUERY_TEMPERATURE_STATUS, qtsp);
            

            // start an exposure
            SBIG.StartExposureParams2 sep = new SBIG.StartExposureParams2();
            sep.ccd = SBIG.CCD.Imaging;
            sep.abgState = SBIG.AbgState.Off;
            sep.openShutter = SBIG.ShutterState.Unchanged;
            sep.exposureTime = 100;
            //sep.width = 765;
            //sep.height = 510;
            sep.width = 1530;
            sep.height = 1020;
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_START_EXPOSURE, sep);


            // read out the image
            ushort[,] img = SBIG.WaitEndAndReadoutExposure(sep);
            //FitsUtil.WriteFitsImage("simcam.fits", img);
            //SBIG.SaveImageToVernacularFormat(sep, img, "foo.gif", ImageFormat.Gif);
                        
            //
            // setup for TDI
            //
            SBIG.MiscellaneousControlParams mcp = new SBIG.MiscellaneousControlParams();
            mcp.fanEnable = 1;
            mcp.ledState = SBIG.LedState.BlinkHigh;
            mcp.shutterCommand = SBIG.ShutterState.OpenForExposureCloseForReadout;
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_MISCELLANEOUS_CONTROL, mcp);

            // turn off pipelining for USB connected cameras
            SBIG.SetDriverControlParams sdcp = new SBIG.SetDriverControlParams();
            sdcp.controlParameter = SBIG.DriverControlParam.DCP_USB_FIFO_ENABLE;
            sdcp.controlValue = 0;
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_SET_DRIVER_CONTROL, sdcp, null);

            //
            // read a TDI line
            //
            // input params
            SBIG.ReadoutLineParams rlp = new SBIG.ReadoutLineParams();
            rlp.ccd = SBIG.CCD.Imaging;
            rlp.pixelStart = 0;
            rlp.pixelLength = 1530;
            rlp.readoutMode = SBIG.ReadoutLineParams.MakeNBinMode(SBIG.ReadoutMode.BinNx1, 4);
            // output
            ushort[] data = new ushort[rlp.pixelLength];
            // make the call!!!
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_READOUT_LINE, rlp, data);
            // do it a lot
            DateTime start = DateTime.Now;
            for (int i = 0; i < 1000000; i++)
                SBIG.UnivDrvCommand(SBIG.Cmd.CC_READOUT_LINE, rlp, data);
            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            Console.WriteLine(ts);

            // clean up
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_CLOSE_DEVICE, null);
            SBIG.UnivDrvCommand(SBIG.Cmd.CC_CLOSE_DRIVER, null);
        } // Main

    } // class
} // namespace
