using System;
using System.IO;
namespace Automatas
{
    public class Stack
    {
        int maxElementos;
        int ultimo;
        float[] elementos;

        public Stack(int maxElementos)
        {
            this.maxElementos = maxElementos;
            ultimo = 0;
            elementos = new float[maxElementos];
        }

        public void push(float element, StreamWriter bitacora, int linea, int caracter)
        {
            if (ultimo < maxElementos)
            {
                bitacora.WriteLine("Push: " + element);
                elementos[ultimo++] = element;
            }
            else
            {
                //else levantar excepcion de stackoverflow

                throw new Error(bitacora, "StackOverflow: El stack está lleno, no se pueden agregar mas elementos" + "(" + linea + ", " + caracter + ")");

            }
        }

        public float pop(StreamWriter bitacora, int linea, int caracter)
        {
            if (ultimo > 0)
            {
                bitacora.WriteLine("Pop: " + elementos[ultimo - 1]);
                return elementos[--ultimo];
            }
            else
            {
                //else levantar excepcion de stackunderflow
                throw new Error(bitacora, "StackUnderflow: El stack está vacío, no se pueden sacar elementos" + "(" + linea + ", " + caracter + ")");
            }
        }

        public void display(StreamWriter bitacora)
        {
            bitacora.WriteLine("Contenido del stack");
            for (int i = 0; i < ultimo; i++)
            {
                bitacora.Write(elementos[i] + " ");
            }
            bitacora.WriteLine("");
        }
    }
}