// translated with https://pythoncsharp.com/ from Python source to C#
// then manually fixed to make it work
//
// (c) 2021 Ian Leiman, ian.leiman@gmail.com
// vdw_calc.py -> 
// GNU General Public License v3.0
// use at your own risk, no guarantees, no liability!
// github project https://github.com/eianlei
//
// C# functions:
//    vdw_calc() calculates trimix blending using Van der Waals gas law instead of ideal gas law
//    this is a quick and dirty implementation that needs more testing
//    and there is no error checking what so ever, so crashes are more than likely

// PYTHON used this -> using fsolve = scipy.optimize.fsolve;
// for C# reusing RootFinding.Bisect() 
// using System.Numerics;
// using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Linq;
using FillCalcWin;

namespace FillCalcWin
{
    public static class VanDerWaals
    {
        static double R = 0.0831451;

        public static double ideal_gas_p(double n, double V, double T)
        {

            return n * R * T / V;
        }

        public static double ideal_gas_n(double p, double V, double T)
        {
            return p * V / (R * T);
        }

        // implementation of the Van der Waals equation
        // https://en.wikipedia.org/wiki/Van_der_Waals_equation
        public class VdW_eqation
        {
            public double p;
            public double n;
            public double V;
            public double T;
            public double a;
            public double b;
            public VdW_eqation(double p, double n, double V, double T, double a, double b)
            {
                this.p = p;
                this.n = n;
                this.V = V;
                this.T = T;
                this.a = a;
                this.b = b;
            }
            public double solve_pressure(double pressure)
            {
                double f = (pressure + n * n * a / (V * V)) * (V - n * b) - n * R * T;
                return f;
            }
            public double solve_mols(double mols)
            {
                double f = (p + mols * mols * a / (V * V)) * (V - mols * b) - mols * R * T;
                return f;
            }
        }

        //  calculate vdw coefficients a and b for a mix of O2, N2, He
        //     input the fractions of each gas, return tuple
        //     
        public static (double, double) vdw_mix_ab(double o2_f, double he_f, double n2_f)
        {
            double mix_a = 0.0;
            double mix_b = 0.0;
            List<double> x = new List<double> {
                o2_f,
                n2_f,
                he_f
            };
            // https://en.wikipedia.org/wiki/Van_der_Waals_constants_(data_page)
            List<double> a = new List<double> {
                1.382,
                1.37,
                0.0346
            };
            List<double> b = new List<double> {
                0.03186,
                0.0387,
                0.0238
            };
            foreach (var i in Enumerable.Range(0, 3))
            {
                foreach (var j in Enumerable.Range(0, 3))
                {
                    mix_a += Math.Sqrt(a[i] * a[j]) * x[i] * x[j];
                    mix_b += Math.Sqrt(b[i] * b[j]) * x[i] * x[j];
                }
            }
            (double, double) result = (mix_a, mix_b);
            return result;
        }

        // 
        //     returns the pressure of a gas mixture of o2, he, n2 by solving Van der Waals equation
        //     given mols, volume and temperature in Celsius
        //     
        public static double vdw_solve_pressure(
            double mols,
            double volume,
            double o2_f,
            double he_f,
            double n2_f,
            double temperature)
        {
            double temp_K = temperature + 273.0;
            var _tup_1 = vdw_mix_ab(o2_f, he_f, n2_f);
            double mix_a = _tup_1.Item1;
            double mix_b = _tup_1.Item2;
            double seed_p = ideal_gas_p(n: mols, V: volume, T: temp_K);
            double solved_p = 0;

            // fsolve not available, as we do not have scipy library in C# as such
            // var solved_p = fsolve(van_der_waals_p, seed_p, (mols, volume, temp_K, mix_a, mix_b));

            // instead use algorithms at RootFinding.cs module, reused from
            // https://www.codeproject.com/Articles/79541/Three-Methods-for-Root-finding-in-C
            VdW_eqation vdw_eq = new VdW_eqation(p: seed_p, n: mols, V: volume, T: temp_K, a: mix_a, b: mix_b);
            try
            {
                solved_p = RootFinding.Bisect(vdw_eq.solve_pressure, seed_p * 0.6, seed_p * 1.5, 0.001, 0.1);
            }
            catch (Exception e)
            {
                solved_p = -1;
                Console.WriteLine(e.Message);
            }

            return solved_p;
        }

 

        // 
        //     returns the total mols in a gas mixture of o2, he, n2 by solving Van der Waals equation
        //     given pressure, volume and temperature in Celsius
        //     
        public static double vdw_solve_mols(
            double pressure,
            double volume,
            double o2_f,
            double he_f,
            double n2_f,
            double temperature)
        {
            var temp_K = temperature + 273.0;
            var _tup_1 = vdw_mix_ab(o2_f, he_f, n2_f);
            var mix_a = _tup_1.Item1;
            var mix_b = _tup_1.Item2;
            var seed_n = ideal_gas_n(pressure, volume, temp_K);

            // fsolve not available, as we do not have scipy library in C# as such
            // var solved_n = fsolve(van_der_waals_n, seed_n, (pressure, volume, temp_K, mix_a, mix_b));

            // instead use algorithms at RootFinding.cs moduel, reused from
            // https://www.codeproject.com/Articles/79541/Three-Methods-for-Root-finding-in-C
            VdW_eqation vdw_eq = new VdW_eqation(p: pressure, n: seed_n, V: volume, T: temp_K, a: mix_a, b: mix_b);
            double solved_n = RootFinding.Bisect(vdw_eq.solve_mols, seed_n * 0.6, seed_n *1.5, 0.001, 0.1);


            return solved_n;
        }

        // 
        //     calculates a partial pressure gas fill using Van der Waals gas law
        public class VdW_Result
        {
            public int status_code;
            public string status_txt;
            public double fill_o2_bars;
            public double fill_he_bars;
            public double fill_air_bars;
        }
        //     
        public static VdW_Result vdw_calc(
            double start_bar = 0.0,
            double want_bar = 200.0,
            double start_o2 = 21.0,
            double start_he = 35.0,
            double want_o2 = 21.0,
            double want_he = 35.0,
            double volume = 12.0,
            double start_temp_c = 20.0)
        {
            VdW_Result vdw_result = new VdW_Result
            {
                status_code = 99,
                status_txt = "FATAL ERROR\n"
            };
            double vdw_start_mols_all = 0;
            double vdw_start_mols_o2 = 0;
            double vdw_start_mols_he = 0;
            double vdw_start_mols_n2 = 0;
            double vdw_want_mols_o2 = 0;
            double vdw_want_mols_he = 0;
            double vdw_want_mols_n2 = 0;

            // assume end temperature is same as start, could change this later to make things more complicated
            double end_temp_c = start_temp_c;

            // convert percentages to fractions (_f)
            double start_o2_f = start_o2 / 100.0;
            double start_he_f = start_he / 100.0;
            double start_n2_f = 1.0 - start_o2_f - start_he_f;
            double want_o2_f = want_o2 / 100.0;
            double want_he_f = want_he / 100.0;
            double want_n2_f = 1.0 - want_o2_f - want_he_f;

            // some basic sanity checks
            if (want_o2==21 & want_he == 0)
            {
                vdw_result.status_txt = "ERROR 01: just filling air";
                vdw_result.status_code = 1;
                return vdw_result;
            }

            // how many mols of gas we have at start, in total and of each kind
            if (start_bar > 0)  // vdw_solve_mols() will crash if you try with 0 bars
            {
                vdw_start_mols_all = vdw_solve_mols(start_bar, volume, start_o2_f, start_he_f, start_n2_f, start_temp_c);
                vdw_start_mols_o2 = vdw_start_mols_all * start_o2_f;
                vdw_start_mols_he = vdw_start_mols_all * start_he_f;
                vdw_start_mols_n2 = vdw_start_mols_all * start_n2_f;
            } else
            {
                // empty tank, just keep the 0 values we already declared
            }

            // how many mols of gas we want to have, in total and of each kind
            double vdw_want_mols_all = vdw_solve_mols(want_bar, volume, want_o2_f, want_he_f, want_n2_f, end_temp_c);
            if (vdw_want_mols_all > 0)
            {
                vdw_want_mols_o2 = vdw_want_mols_all * want_o2_f;
                vdw_want_mols_he = vdw_want_mols_all * want_he_f;
                vdw_want_mols_n2 = vdw_want_mols_all * want_n2_f;
            } else
            {
                vdw_result.status_txt = "ERROR 11: vdw_want_mols_all";
                vdw_result.status_code = 11;
                return vdw_result;
            }

            // how many mols we need to fill
            double vdw_fill_mols_all = vdw_want_mols_all - vdw_start_mols_all;
            double vdw_fill_mols_o2 = vdw_want_mols_o2 - vdw_start_mols_o2;
            double vdw_fill_mols_he = vdw_want_mols_he - vdw_start_mols_he;
            double vdw_fill_mols_n2 = vdw_want_mols_n2 - vdw_start_mols_n2;

            // first stage of filling is by helium, and we get a new mix "mix_he"
            var mix_he_mols_all = vdw_start_mols_all + vdw_fill_mols_he;
            var mix_he_o2_f = vdw_start_mols_o2 / mix_he_mols_all;
            var mix_he_he_f = vdw_want_mols_he / mix_he_mols_all;
            var mix_he_n2_f = 1.0 - mix_he_o2_f - mix_he_he_f;

            // then solve for pressure of this new mix
            var mix_helium_bars = vdw_solve_pressure(mix_he_mols_all, volume, mix_he_o2_f, mix_he_he_f, mix_he_n2_f, start_temp_c);
            var vdw_fill_he_bars = mix_helium_bars - start_bar;

            // air is topped last, but we need to calculate how much we need it, so we can calculate for oxygen
            var air_o2_mols_o2 = vdw_fill_mols_n2 * (0.21 / 0.79);
            var mix_o2_mols_o2 = vdw_fill_mols_o2 - air_o2_mols_o2;
            var mix_o2_mols_all = mix_he_mols_all + mix_o2_mols_o2;
            var mix_o2_o2_f = (mix_o2_mols_o2 + vdw_start_mols_o2) / mix_o2_mols_all;
            var mix_o2_he_f = vdw_want_mols_he / mix_o2_mols_all;
            var mix_o2_n2_f = 1.0 - mix_o2_o2_f - mix_o2_he_f;

            // then solve for pressure of this new mix
            var mix_oxygen_bars = vdw_solve_pressure(mix_o2_mols_all, volume, mix_o2_o2_f, mix_o2_he_f, mix_o2_n2_f, start_temp_c);
            var vdw_fill_o2_bars = mix_oxygen_bars - mix_helium_bars;

            // finally air
            var vdw_fill_air_bars = want_bar - mix_oxygen_bars;
            vdw_result.fill_he_bars = vdw_fill_he_bars;
            vdw_result.fill_o2_bars = vdw_fill_o2_bars;
            vdw_result.fill_air_bars = vdw_fill_air_bars;

            // build the string for return
            string start_mix = $"Starting from {start_bar} bar with mix {start_o2:F0}/{start_he:F0} (O2/He).";
            string result_mix = $"Resulting mix will be {want_o2:F0}/{want_he:F0} (O2/He).";
            string he_fill = $"From {start_bar:F1} bars add {vdw_fill_he_bars:F1} bar Helium,";
            string o2_fill = $"From {mix_helium_bars:F1} bars add {vdw_fill_o2_bars:F1} bar Oxygen,";
            string result =
                $"{start_mix}\n" +
                $"Van der Waals blend:\n" +
                $"- {he_fill}\n" +
                $"- {o2_fill}\n" +
                $"- From {mix_oxygen_bars:F1} bars add {vdw_fill_air_bars:F1} bar air to {want_bar:F1} bar.  \n" +
                $"{result_mix}\n";

            vdw_result.status_txt = result;
            vdw_result.status_code = 0;
            return vdw_result;
        }
    }
}
