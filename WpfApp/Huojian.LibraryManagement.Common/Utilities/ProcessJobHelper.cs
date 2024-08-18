using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class ProcessJobHelper
    {
        private static WindowsJobObject _jobObject;

        public static void ConfigureJobObject(WindowsJobCreateOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("JobObject options is null");
            if (_jobObject != null)
                throw new InvalidOperationException("JobObject is already configured");

            _jobObject = new WindowsJobObject(options);
        }

        public static void AddProcessToJob(int pid)
        {
            ValidateJobObject();
            _jobObject.AddProcess(pid);
        }

        public static Process CreateProcessWithJob(string fileName, string arguments = null)
        {
            ValidateJobObject();
            var process = Process.Start(fileName, arguments);
            _jobObject.AddProcess(process.Id);
            return process;
        }

        public static Process CreateProcessWithJob(ProcessStartInfo startInfo)
        {
            ValidateJobObject();
            var process = Process.Start(startInfo);
            _jobObject.AddProcess(process.Id);
            return process;
        }

        private static void ValidateJobObject()
        {
            if (_jobObject == null)
            {
                throw new InvalidOperationException("JobObject is not configured");
            }
        }
    }
}
