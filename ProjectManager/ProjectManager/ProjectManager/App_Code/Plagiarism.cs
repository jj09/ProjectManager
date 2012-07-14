using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManager.Helpers
{
    public static class Plagiarism
    {
        public static int power_modulo_fast(int a, int b, int m)
        {
            int i;
            int result = 1;
            int x = a % m;

            for (i = 1; i <= b; i <<= 1)
            {
                x %= m;
                if ((b & i) != 0)
                {
                    result *= x;
                    result %= m;
                }
                x *= x;
            }

            return result % m;
        }

        public static bool search(string txt, string pattern)
        {
            int m, n, i, j, h1, h2, rm;
            const int r = 256;   //liczba symboli alfabetu (char 0-255)
            const int q = 127;  //9551;  //mo¿liwie du¿a liczba pierwsza

            n = txt.Length;
            m = pattern.Length;
            h2 = 0;
            h1 = 0;

            //algorytm Hornera do obliczenia funkcji haszujacej h(tekst[1..m])
            for (i = 0; i < m; i++)
            {
                h2 = ((h2 * r) + txt[i]);
                h2 %= q;
            }
            //algorytm Hornera do obliczenia funkcji haszujacej h(wzorzec)
            for (i = 0; i < m; i++)
            {
                h1 = ((h1 * r) + pattern[i]);
                h1 %= q;
            }

            rm = power_modulo_fast(r, m - 1, q);
            i = 0;
            while (i < n - m)
            {
                j = 0;
                if (h1 == h2)
                {
                    while ((j < m) && (pattern[j] == txt[i + j]))
                        j++;
                }
                if (j == m)
                    return true;
                h2 = ((h2 - txt[i] * rm) * r + txt[i + m]);
                h2 %= q;
                if (h2 < 0)
                    h2 += q;
                i++;
            }
            j = 0;
            if (h1 == h2)
            {
                while ((j < m) && (pattern[j] == txt[i + j]))
                    j++;
            }
            if (j == m)
                return true;

            return false;

        }
    }
}