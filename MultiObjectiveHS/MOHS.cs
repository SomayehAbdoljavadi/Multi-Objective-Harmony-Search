using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/*
 IF YOU USE THIS CODE, YOU CAN CITE THE FOLLOWING WORKS:
 [1] M. Fesanghary, S. Asadi, Z.W. Geem, Design of Low-Emission and Energy-Efficient Residential Buildings Using a Multi-Objective Optimization Algorithm, Building and Environment 49 (2012) 245-250
 
  
This code can support both real and discrete decision variables. The array "Type" control the variable type,
 if Type[i] = 1 it means X_i is real, if Type[i] = 0 it means X_i is discrete (Integer, or real!).
 
 The range of variables for discrete and real variables should be entered separately. The array "low" and "high" are used for real parameters. 
 The range for each discrete parameter should be defined as an array first. Then the arrays should be added to the array list "Ranges" in turn. 
 Example:
   double[] var3 = { 0.003, 0.006, 0.008, 0.013 };
   double[] var4 = { 5, 13, 18, 22 };
   hs.Ranges.Add(var3);
   hs.Ranges.Add(var4);
 
 
*/

public class MOHS
    {


        #region Fields

        public ArrayList Ranges = new ArrayList();
        public double[,] ParetoSet;
        public int VarType { get; set; }
        public int nObjectives { get; set; }
        public int[] Type { get; set; }
        public int NVAR { get; set; }
        public int HMS { get; set; }
        public int maxIter { get; set; }
        public double PAR { get; set; }
        public double PARmax { get; set; }
        public double PARmin { get; set; }
        public double BW { get; set; }
        public double IntegerBW { get; set; }
        public bool IntegerPermuteRange { get; set; }
        public bool Flip { get; set; }
        public double BWmax { get; set; }
        public double BWmin { get; set; }
        public bool isVariableBW { get; set; }
        public bool isVariablePAR { get; set; }
        public double HMCR { get; set; }
        public double runTime { get; set; }
        private double[] low;
        private double[] high;
        private double[] NCHV;
        private int[] lowInt;
        private int[] highInt;
        //private double[] NCHV;
        private double[] bestFitHistory;
        public double[] bestHarmony { get; set; }
        private double[] worstFitHistory;
        private double[,] HM;
        public int generation { get; set; }
        private bool terminationCriteria = true;
        RandomGenerator randGen = new RandomGenerator(System.DateTime.Now.Ticks);
        private IFunction fit;
        public interface IFunction
        {
            double[] F(double[] x);
        }
        int[] indices;
        
        #endregion

        public MOHS()
        {
            // default parameters
            BW = 0.02;
            NVAR = 5;
            HMCR = .9;
            HMS = 5;
            PAR = .4;
            maxIter = 10000;
            generation = 0;
            isVariableBW = isVariablePAR = false;
            VarType = 1;  // double
            nObjectives = 1;
        }

        private void setArrays()
        {
            low = new double[NVAR];
            high = new double[NVAR];
            lowInt = new int[NVAR];
            highInt = new int[NVAR];

            NCHV = new double[NVAR];
            bestHarmony = new double[NVAR + 1];
            bestFitHistory = new double[maxIter + 1];
            worstFitHistory = new double[maxIter + 1];
            HM = new double[HMS, NVAR + nObjectives];
        }

        public void setBounds(double[] low, double[] high)
        {
            setArrays();
            this.low = low;
            this.high = high;
        }

        public void setBounds(int[] low, int[] high)
        {
            setArrays();
            this.lowInt = low;
            this.highInt = high;

         }

        private void initiator()
        {
            int i;
            double[] curFit;

            // save the location index of integer parameters in the array list, "Ranges"
            int count=0;
            indices = new int[NVAR];
            for ( i = 0; i < NVAR; i++)
                if (Type[i] == 0)
                    indices[i] = count++;


            if (isVariableBW)
            {
                if (BWmax <= 0) throw new Exception("BWmax should be greater than Zero");
            }
            else
                BWmax = BW;

            if (isVariablePAR)
            {
                if (PARmin <= 0) throw new Exception("PARmin should be greater than Zero"); 
            }
            else
                PARmin  = PAR;

            // not necessary!!
            for (i = 0; i < HMS; i++)
                for (int k = 0; k < nObjectives; k++)
                    HM[i, NVAR + k] = double.MaxValue;

            for (i = 0; i < HMS; i++)
            {
                for (int j = 0; j < NVAR; j++)
                {
                    if (Type[j] == 1)
                        HM[i, j] = randGen.randVal(low[j], high[j]);
                    else
                    {
                        int len = ((double[])(Ranges.ToArray().ElementAt(indices[j]))).Length;
                        HM[i, j] = ((double[])(Ranges.ToArray().ElementAt(indices[j])))[randGen.randVal(0, len - 1)];
                    }

                    NCHV[j] = HM[i, j];
                }
                curFit = fitness(NCHV);
                for (int k = 0; k < nObjectives; k++)
                    HM[i, NVAR + k] = curFit[k];
            }
        }

        public double[] fitness(double[] x)
        {
            return fit.F(x);
        }

        public bool stopCondition()
        {
            if (generation > maxIter)
                terminationCriteria = false;
            return terminationCriteria;

        }

        public void updateHarmonyMemory(double[] newFitness)
        {
            if (nObjectives == 1)
            {
                // find worst harmony
                double worst = HM[0, NVAR];
                int worstIndex = 0;
                for (int i = 0; i < HMS; i++)
                    if (HM[i, NVAR] > worst)
                    {
                        worst = HM[i, NVAR];
                        worstIndex = i;
                    }
                worstFitHistory[generation] = worst;
                // update harmony
                if (newFitness[0] < worst)
                {
                    for (int k = 0; k < NVAR; k++)
                        HM[worstIndex, k] = NCHV[k];
                    HM[worstIndex, NVAR] = newFitness[0];
                }

                // find best harmony
                double best = HM[0, NVAR];
                int bestIndex = 0;
                for (int i = 0; i < HMS; i++)
                    if (HM[i, NVAR] < best)
                    {
                        best = HM[i, NVAR];
                        bestIndex = i;
                    }
                bestFitHistory[generation] = best;
                if (1 > 0 /*generation > 0 && best != bestFitHistory[generation - 1]*/)
                {
                    for (int k = 0; k < NVAR; k++)
                        bestHarmony[k] = HM[bestIndex, k];
                    bestHarmony[NVAR] = best;
                }
            }
            else
            {
                int sum = 0;
                for (int i = 0; i < HMS; i++)
                {
                    sum = 0;
                    for (int j = 0; j < nObjectives; j++)
                        if (newFitness[j] <= HM[i, NVAR + j])
                            sum++;
                    if (sum == nObjectives)
                    {
                        for (int j = 0; j < nObjectives; j++)
                            HM[i, NVAR + j] = newFitness[j];
                        for (int k = 0; k < NVAR; k++)
                            HM[i, k] = NCHV[k];
                        break;
                    }
                }




            }
        }

        private void memoryConsideration(int varIndex)
        {

            NCHV[varIndex] = HM[randGen.randVal(0, HMS - 1), varIndex];
        }

        private void pitchAdjustment(int varIndex)
        {

            double rand = randGen.ran1();
            double temp = NCHV[varIndex];

            if (Type[varIndex] == 1)   /* REAL VARIABELS  */ 
            {
                if (rand < 0.5)
                {
                    temp += rand * GetBW();
                    if (temp < high[varIndex])
                        NCHV[varIndex] = temp;
                }
                else
                {
                    temp -= rand * GetBW();
                    if (temp > low[varIndex])
                        NCHV[varIndex] = temp;
                }
            }
            else /* NON-REAL VARIABELS  */ 
            {
                if (IntegerPermuteRange)
                {

                    int len = ((double[])(Ranges.ToArray().ElementAt(indices[varIndex]))).Length;
                    NCHV[varIndex] = ((double[])(Ranges.ToArray().ElementAt(indices[varIndex])))[randGen.randVal(0, len - 1)];

                }
                else if (Flip)  // for 0,1 cases
                    if (NCHV[varIndex] == 0)
                        NCHV[varIndex] = 1;
                    else
                        NCHV[varIndex] = 0;
                else
                {

                    if (rand < 0.5)
                    {
                        temp += IntegerBW;
                        if (temp < highInt[varIndex])
                            NCHV[varIndex] = temp;
                    }
                    else
                    {
                        temp -= IntegerBW;
                        if (temp > lowInt[varIndex])
                            NCHV[varIndex] = temp;
                    }
                }

            }

        }

        private void randomSelection(int varIndex)
        {
            if (Type[varIndex] == 1)
                NCHV[varIndex] = randGen.randVal(low[varIndex], high[varIndex]);
            else
            {
                int len = ((double[])(Ranges.ToArray().ElementAt(indices[varIndex]))).Length;
                NCHV[varIndex] = ((double[])(Ranges.ToArray().ElementAt(indices[varIndex])))[randGen.randVal(0, len - 1)];
            }
        }

        public void Solve(IFunction f)
        {
            DateTime t1 = DateTime.Now;
            this.fit = f;
            initiator();

            while (stopCondition())
            {

                for (int i = 0; i < NVAR; i++)
                {
                    if (randGen.ran1() < HMCR)
                    {
                        memoryConsideration(i);
                        if (randGen.ran1() < PAR)
                            pitchAdjustment(i);
                    }
                    else
                        randomSelection(i);
                }

                double[] currentFit;
                currentFit = fitness(NCHV);
                updateHarmonyMemory(currentFit);
                generation++;
            }

            DateTime t2 = DateTime.Now;
            TimeSpan ts = t2 - t1;
            runTime = ts.TotalSeconds;

        }

        private double GetBW()
        {
            double val;
            double c = Math.Log(BWmin / BWmax) / maxIter;
            if (isVariableBW)
                val = BWmax * Math.Exp(c * generation);
            else
                val = BWmax;
            return val;


        }

        private double GetPAR()
        {
            double val;
            if (isVariablePAR)
                val = (PARmax - PARmin) / (maxIter) * generation + PARmin;
            else
                val = PARmin;
            return val;
        }

        public void choosePareto()
        {
            ParetoSet = new double[HMS, nObjectives+NVAR];

            int cntr = 0;
            for (int i = 0; i < HMS; i++)
            {
                for (int j = 0; j < HMS; j++)
                {
                    if (i != j && HM[i, NVAR] >= HM[j, NVAR] && HM[i, NVAR + 1] > HM[j, NVAR + 1]) goto L100;
                    if (i != j && HM[i, NVAR] > HM[j, NVAR] && HM[i, NVAR + 1] >= HM[j, NVAR + 1]) goto L100;
                }

                for (int j = 0; j < nObjectives; j++)
                    ParetoSet[cntr, j] = HM[i, NVAR + j];
                for (int j = nObjectives; j < nObjectives + NVAR; j++)
                    ParetoSet[cntr, j] = HM[i, j - nObjectives];
                cntr++;
            L100:
                ;
            }


        }
    }//end class

    internal class RandomGenerator
    {

        const long IA = 16807;
        const long IM = 2147483647;
        const double AM = 1.0 / IM;
        const long IQ = 127773;
        const long IR = 2836;
        const int NTAB = 32;
        const double NDIV = (1 + (IM - 1) / NTAB);
        const double EPS = 1.2e-7;
        const double RNMX = (1.0 - EPS);
        private long iy;
        private long[] iv = new long[NTAB];
        private long idum;



        public RandomGenerator()
        {
            sran1(DateTime.Now.Ticks);
        }

        public RandomGenerator(long seed)
        {
            sran1(seed);
        }

        public void sran1(long seed)
        {
            int j;
            long k;

            idum = seed;
            if (idum < 1) idum = 1;
            for (j = NTAB + 7; j >= 0; j--)
            {
                k = (idum) / IQ;
                idum = IA * (idum - k * IQ) - IR * k;
                if (idum < 0) idum += IM;
                if (j < NTAB) iv[j] = idum;
            }
            iy = iv[0];

        }


        public double ran1()
        {

            int j;
            long k;
            double temp;

            k = (idum) / IQ;
            idum = IA * (idum - k * IQ) - IR * k;
            if (idum < 0) idum += IM;
            j = (int)(iy / NDIV);
            iy = iv[j];
            iv[j] = idum;
            temp = AM * iy;
            if (temp > RNMX) return RNMX;
            else return temp;
        }


        public double randVal(double low, double high)
        {
            return (float)(ran1() * (high - low) + low);
        }


        public int randVal(int low, int high)
        {
            return (int)(Math.Floor(ran1() * (high - low) + low + .5));
        }

    }


