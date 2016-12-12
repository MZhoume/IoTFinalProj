using System;
using System.Collections;
using System.Linq;
using Accord;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;

namespace Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var network = new ActivationNetwork(new BipolarSigmoidFunction(), 1, 1, 4, 4, 1);
            Train(network);

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(Math.Round(network.Compute(new double[] { i / 150.0 })[0] * 150.0, 2));
            }

            network.Save("network.net");


            Console.ReadLine();
        }

        public static void Train(ActivationNetwork network)
        {
            var input = Enumerable.Range(-100, 251)
                      .Select(n => new double[] { n / 150.0 }).ToArray();

            var teacher = new LevenbergMarquardtLearning(network);
            int iteration = 1;

            var samples = input.GetLength(0);

            var needToStop = false;
            var iterations = 3000;

            while (!needToStop)
            {
                // run epoch of learning procedure
                double error = teacher.RunEpoch(input, input) / samples;
                Console.WriteLine($"Iteration {iteration} with error: {error}");

                double learningError = 0.0;
                for (int j = 0; j < samples; j++)
                {
                    double x = input[j][0];
                    double expected = input[j][0];
                    double actual = network.Compute(new[] { x })[0];
                    learningError += Math.Abs(expected - actual);
                }

                // increase current iteration
                iteration++;

                // check if we need to stop
                if ((iterations != 0) && (iteration > iterations))
                    break;
            }
        }
    }
}
