using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


public sealed class Utility
{
    public static string address = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
    static StreamWriter sw2;




    public static void WriteMatrix(double[,] M, string name, string format)
    {

        StreamWriter sw = File.CreateText(address + "\\" + name + ".txt");

        for (int i = 0; i < M.GetLength(0); i++)
            for (int j = 0; j < M.GetLength(1); j++)
            {
                sw.Write("{0}\t", M[i, j].ToString(format));  //"n8"
                if (j == M.GetLength(1) - 1)
                    sw.WriteLine();
            }

        sw.Close();
    }

} 
