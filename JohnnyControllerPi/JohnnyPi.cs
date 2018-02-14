using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JcNetworking;

namespace JohnnyPi
{
    class JohnnyPi
    {

        private string _portname;
        private SerialPort _port;
        private JcRobotNetworking _jcn;

        private float[] _percents;

        private int _time = 50;
        private const int _numMotors = 15;

        private const int _servoWaist = 0;
        private const int _servoTorso = 1;
        private const int _servoHead = 2;
        private const int _servoShoulderLeft = 3;
        private const int _servoShoulderRight = 4;
        private const int _servoElbowLeft = 5;
        private const int _servoElbowRight = 6;
        private const int _servoArmRotationLeft = 7;
        private const int _servoArmRotationRight = 8;
        private const int _servoWristLeft = 9;
        private const int _servoWristRight = 10;
        private const int _servoHandLeft = 11;
        private const int _servoHandRight = 12;
        private const int _motorDriveLeft = 13;
        private const int _motorDriveRight = 14;
        private const int _motorTime = 254;

        public static void Main(string[] args){
            JohnnyPi jc = new JohnnyPi();
            jc.Instantiate();
            jc.Start();
        }

        public JohnnyPi()
        {

            Console.WriteLine("Which serial _port would you like?");


            foreach (string name in SerialPort.GetPortNames())
            {
               //if(name.Contains("USB"))
                    Console.WriteLine(name.Substring(5));
            }
            if (SerialPort.GetPortNames().Length == 1)
            {
                _portname = SerialPort.GetPortNames()[0];
            }
            else
            {
                Console.Write("Your choice: ");
                _portname = Console.ReadLine();
                _portname = "/dev/"+_portname;
            }
            



        }

        public void Instantiate()
        {
            _port = new SerialPort(_portname,115200, Parity.None, 8, StopBits.One);
            _port.Open();
            _jcn = new JcRobotNetworking(JcRobotNetworking.ConnectionType.Robot, Update);

            _percents = new float[_numMotors];
            for (int i = 0; i < _percents.Length; i++)
            {

                _percents[i] = .5f;
            }
            SetPositions(_percents, _time);
            

        }


        public void Start()
        {
            _jcn.Connect(1296);
            new Thread(()=>{Loop();}).Start();


        }

        private void Loop(){
            while(true){
                SetPositions(_percents, _time);
                Thread.Sleep(_time);
            }
        }

        private void Update(JcRobotNetworking.Command c)
        {
            Console.WriteLine("New Command: "+c.commandID+", "+BitConverter.ToSingle(c.param,0));
            if(c.commandID==_motorTime){
                _time = BitConverter.ToInt32(c.param, 0);
            }else{
                SetValue(c.commandID, BitConverter.ToSingle(c.param, 0));
            }
        }

        private void SetValue(int index, float value){
            _percents[index] = value;
            _percents[index] = Minmax(_percents[index], 0, 1);
        }

        private float Minmax(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value>max)
            {
                return max;
            }
            return value;
        }

        private void SetPosition(int motor, float percent, int time)
        {
            _port.Write("#"+motor+"P"+(int)(2000*percent+500)+"T"+time+"\r");
        }

        private void SetPositions( float[] percents, int time, int?[] motors = null)
        {

            string message = "";
            for (int i = 0; i < percents.Length; i++)
            {
                if (motors == null)
                {
                    message += "#" + i + "P" + (int)(2000 * percents[i] + 500);
                }else{
                    message += "#" + motors[i] + "P" + (int)(2000 * percents[i] + 500);
                }

            }
            message += "T" + time;
            _port.Write(message+"\r");
        }


    }
}
