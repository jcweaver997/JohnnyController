using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JcNetworking;

namespace JohnnyController
{
    class JohnnyController
    {

        private bool _running;
        private string _joystickName;
        private string _portname;
        private SerialPort _port;
        private JcRobotNetworking _jcn;

        private float[] _percents;
        private int _time;
        private int _timeOverlap;
        private float _controllerGain;
        private const int numMotors = 15;
        private const float speed0 = .5f;
        private const float speed1 = .75f;
        private const float speed2 = 1;
        private const float speed3 = 1.5f;
        private const float speed4 = 2;
        private const float speed5 = 2.5f;
        private const int servoWaist = 0;
        private const int servoTorso = 1;
        private const int servoHead = 2;
        private const int servoShoulderLeft = 3;
        private const int servoShoulderRight = 4;
        private const int servoElbowLeft = 5;
        private const int servoElbowRight = 6;
        private const int servoArmRotationLeft = 7;
        private const int servoArmRotationRight = 8;
        private const int servoWristLeft = 9;
        private const int servoWristRight = 10;
        private const int servoHandLeft = 11;
        private const int servoHandRight = 12;
        private const int motorDriveLeft = 13;
        private const int motorDriveRight = 14;

        public JohnnyController()
        {

            Console.WriteLine("Which serial _port would you like?");


            foreach (string name in SerialPort.GetPortNames())
            {
               if(name.Contains("USB"))
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

            _jcn.Commands.Add(servoWaist, servoWaist);
            _jcn.Commands.Add(servoTorso, servoTorso);
            _jcn.Commands.Add(servoHead, servoHead);
            _jcn.Commands.Add(servoShoulderLeft, servoShoulderLeft);
            _jcn.Commands.Add(servoShoulderRight, servoShoulderRight);
            _jcn.Commands.Add(servoElbowLeft, servoElbowLeft);
            _jcn.Commands.Add(servoElbowRight, servoElbowRight);
            _jcn.Commands.Add(servoArmRotationLeft, servoArmRotationLeft);
            _jcn.Commands.Add(servoArmRotationRight, servoArmRotationRight);
            _jcn.Commands.Add(servoWristLeft, servoWristLeft);
            _jcn.Commands.Add(servoWristRight, servoWristRight);
            _jcn.Commands.Add(servoHandLeft, servoHandLeft);
            _jcn.Commands.Add(servoHandRight, servoHandRight);
            _jcn.Commands.Add(motorDriveLeft, motorDriveLeft);
            _jcn.Commands.Add(motorDriveRight, motorDriveRight);


            _running = true;
            _time = 50;
            _timeOverlap = 2;
            _controllerGain = .1f/(1000/_time);
            _percents = new float[numMotors];
            for (int i = 0; i < _percents.Length; i++)
            {

                _percents[i] = .5f;
            }
            SetPositions(_percents, _time);
            

        }


        public void Start()
        {
            _jcn.Connect(1296);
            while (_running)
            {
                TestUpdate();
                System.Threading.Thread.Sleep(_time-_timeOverlap);
            }
        }

        private void TestUpdate(){
            SetPositions(_percents, _time);
        }

        private void Update(JcRobotNetworking.Command c)
        {
            
            SetPositions(_percents, _time);
        }

        private void SetValue(int index, float value){
            _percents[index] = value;
            _percents[index] = minmax(_percents[index], 0, 1);
        }

        private float minmax(float value, float min, float max)
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
