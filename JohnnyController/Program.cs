using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JohnnyController
{
    class Program
    {
         static void Main(string[] args)
         {
            Console.WriteLine("Number of robot arms: ");
            int num = int.Parse(Console.ReadLine());

            for (int i = 0; i < num; i++){
                
                JohnnyController ac = new JohnnyController();
                ac.Instantiate();
                new Thread(() => { ac.Start(); }).Start();
            }

         }
    }

   
}
