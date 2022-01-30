using System;

namespace ConnectGame.Runner
{
    class Elo
    {
        int m_wins;
        int m_losses;
        int m_draws;
        double m_mu;
        double m_stdev;

        public Elo(int wins, int losses, int draws)
        {
            m_wins = wins;
            m_losses = losses;
            m_draws = draws;

            double n = wins + losses + draws;
            double w = wins / n;
            double l = losses / n;
            double d = draws / n;
            m_mu = w + d / 2.0;

            double devW = w * Math.Pow(1.0 - m_mu, 2.0);
            double devL = l * Math.Pow(0.0 - m_mu, 2.0);
            double devD = d * Math.Pow(0.5 - m_mu, 2.0);
            m_stdev = Math.Sqrt(devW + devL + devD) / Math.Sqrt(n);
        }

        double pointRatio()
        {

            double total = (m_wins + m_losses + m_draws) * 2;
            return ((m_wins * 2) + m_draws) / total;
        }

        double drawRatio()
        {
            double n = m_wins + m_losses + m_draws;
            return m_draws / n;
        }

        public double diff()
        {
            return diff(m_mu);
        }

        double diff(double p)
        {
            return -400.0 * Math.Log10(1.0 / p - 1.0);
        }

        public double errorMargin()
        {
            double muMin = m_mu + phiInv(0.025) *m_stdev;
            double muMax = m_mu + phiInv(0.975) * m_stdev;
            return (diff(muMax) - diff(muMin)) / 2.0;
        }

        double erfInv(double x)
        {
            const double pi = 3.1415926535897;

            double a = 8.0 * (pi - 3.0) / (3.0 * pi * (4.0 - pi));
            double y = Math.Log(1.0 - x * x);
            double z = 2.0 / (pi * a) + y / 2.0;

            double ret = Math.Sqrt(Math.Sqrt(z * z - y / a) - z);

            if (x < 0.0)
                return -ret;
            return ret;
        }

        double phiInv(double p)
        {
            return Math.Sqrt(2.0) * erfInv(2.0 * p - 1.0);
        }

        public double LOS()
        {
            return 100 * (0.5 + 0.5 * Erf((m_wins - m_losses) / Math.Sqrt(2.0 * (m_wins + m_losses))));
        }

        double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
    }
}