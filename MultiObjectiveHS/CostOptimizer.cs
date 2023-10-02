/*
 IF YOU USE THIS CODE, YOU CAN CITE THE FOLLOWING WORK:
 [1] M. Fesanghary, S. Asadi, Z.W. Geem, Design of Low-Emission and Energy-Efficient Residential Buildings Using a Multi-Objective Optimization Algorithm, Building and Environment 49 (2012) 245-250
   
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MultiObjectiveHS
{

    public class CostOptimizer : MOHS.IFunction
    {

        #region Fields

        MOHS hs;
        int nVar;
        double[] xlb;
        double[] xub;
        int Iteration = 0;
        public string address = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        StreamWriter sw1;
        double[] bestSolution;
        string str1 = DateTime.Now.ToLongDateString();
        string str2 = (DateTime.Now.Hour).ToString();
        string str3 = (DateTime.Now.Minute).ToString();
        string str4 = (DateTime.Now.Second).ToString();
        string name2, name1;
        
        #endregion

        public void Solve()
        {
            name1 = address + "\\Report.txt";

            nVar = 2;  // number of variables

            xlb = new double[nVar];
            xub = new double[nVar];


            RunHS();

        }


        public double[] F(double[] x)
        {
            double[] f = new double[hs.nObjectives];
            Iteration++;
            try
            {

                f[0] = x[0] + Math.Exp(x[1]);                        // objective 1 = x0 + exp(x1) 
                f[1] = -2 * x[0] + Math.Exp(-x[1]);                  // objective 2 = -2x0 + exp(-x1) 

            }
            catch (Exception e) // in case of error...! //Somi*****
            {
                f[0] = f[1] = double.MaxValue;
                Console.WriteLine(e.Message);
            }

            #region Report

            try
            {
                if (Iteration == 1) { File.Delete(name1); }

                sw1 = File.AppendText(name1);
                sw1.Write(Iteration.ToString() + "\t");
                sw1.Write(f[0].ToString("e") + "\t" + f[1].ToString("e") + "\t");
                for (int i = 0; i < bestSolution.Length - 2; i++)
                    sw1.Write(x[i].ToString() + "\t");
                sw1.WriteLine();
                sw1.Close();

                if (Iteration % 10 == 0)
                    Console.WriteLine("# " + Iteration.ToString() + "  f0 = " + (f[0]).ToString("e") + "  f1 = " + (f[1]).ToString("e"));

                if (Iteration % 50 == 0)
                {
                    hs.choosePareto();
                    Utility.WriteMatrix(hs.ParetoSet, "Pareto Front", "e3");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            #endregion


            return f;
        }


        public void RunHS()
        {
            
            hs = new MOHS();
            //hs.BW = 5.0e-2;
            hs.nObjectives = 2;   // I HAVE NOT TESTED THIS CODE FOR MORE THAN 2 OBJECTIVES!!!
            hs.isVariableBW = true;
            hs.isVariablePAR = true;
            hs.BWmax = 5e-1;
            hs.BWmin = 5e-3;
            hs.PARmin = .4;
            hs.PARmax = .9;
            hs.NVAR = nVar;
            hs.HMCR = .8;
            hs.HMS = 80;
            hs.maxIter = 10000;
            hs.setBounds(xlb, xub);
            hs.VarType = 0;  // integer   
            //hs.IntegerBW = 1;  // 
            //hs.IntegerPermuteRange = true;
            int[] type = new int[nVar];
            bestSolution = new double[nVar + hs.nObjectives];

            #region SetVariableRange


            type[0] = 0;
            double[] var0 = { 1,-5,2,5,9,7 }; // allowable values for x0
            hs.Ranges.Add(var0);

            //xlb[0] = 0;
            //xub[0] = 0.0001;


            type[1] = 1;  // real
           //   1 < x1 < 4
            xlb[1] = 0;
            xub[1] = 3;

            #endregion

            hs.Type = type;
            hs.Solve(this);
        }


      

    }
}
