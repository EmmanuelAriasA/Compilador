using System;
using System.Collections.Generic;
using System.Text;

// ✓ Requerimiento 1: Implementar el not en el if.
//
// ✓ Requerimiento 2: Validar la asignación de strings en instrucción.
//
// ✓ Requerimiento 3: Implementar la comparación de tipo de datos en Lista_IDs.
// 
// ✓ Requerimiento 4: Validar los tipos de datos en la asignación del cin.
//
// ✓ Requerimiento 5: Implementar el cast.
// 

namespace Automatas
{
    class Lenguaje : Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo MaxBytes;
        public Lenguaje()
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre) : base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        // Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);

                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }

                match(">");

                Libreria();
            }
        }

        // Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");

            BloqueInstrucciones(true);
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {

            match(clasificaciones.inicioBloque);

            Instrucciones(ejecuta);

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(Variable.tipo TIPO, bool ejecuta)
        {
            string nombre = getContenido();
            if (!l.Existe(nombre))
            {
                match(clasificaciones.identificador);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") está duplicada" + "(" + linea + ", " + caracter + ")");
            }
            l.Inserta(nombre, TIPO);


            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);

                if (getClasificacion() == clasificaciones.cadena)
                {
                    if (TIPO == Variable.tipo.STRING)
                    {
                        if (ejecuta)
                        {
                            string cadena = getContenido();
                            l.setValor(nombre, cadena);
                        }
                        match(clasificaciones.cadena);
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico1: No se puede asignar un STRING a un (" + TIPO + ")" + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //Requerimiento 3
                    Expresion();
                    MaxBytes = Variable.tipo.CHAR;

                    string valor;
                    valor = s.pop(bitacora, linea, caracter).ToString();

                    if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
                    {
                        MaxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if (MaxBytes > TIPO)
                    {
                        throw new Error(bitacora, "Error semantico2: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + "(" + linea + ", " + caracter + ")");
                    }
                    if (ejecuta)
                    {
                        l.setValor(nombre, valor);
                    }
                }
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(TIPO, ejecuta);
            }

        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            string tipoDato = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo Tipo;
            switch (tipoDato)
            {
                case "char":
                    Tipo = Variable.tipo.CHAR;
                    break;

                case "float":
                    Tipo = Variable.tipo.FLOAT;
                    break;

                case "int":
                    Tipo = Variable.tipo.INT;
                    break;

                case "string":
                    Tipo = Variable.tipo.STRING;
                    break;

                default:
                    Tipo = Variable.tipo.CHAR;
                    break;
            }
            Lista_IDs(Tipo, ejecuta);
            match(clasificaciones.finSentencia);
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DoWhile(ejecuta);
            }
            else if (getContenido() == "while")
            {
                While(ejecuta);
            }
            else if (getContenido() == "for")
            {
                For(ejecuta);
            }
            else if (getContenido() == "if")
            {
                If(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                // Requerimiento 4
                match("cin");
                match(clasificaciones.flujoEntrada);

                string nombre = getContenido();
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada" + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    if (ejecuta)
                    {
                        match(clasificaciones.identificador); // Validar existencia
                        string valor = Console.ReadLine();

                        if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
                        {
                            MaxBytes = tipoDatoExpresion(float.Parse(valor));
                        }

                        if (MaxBytes > l.getTipoDato(nombre))
                        {
                            throw new Error(bitacora, "Error semantico3: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + "(" + linea + ", " + caracter + ")");

                        }
                        l.setValor(nombre, valor);
                    }
                }

                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
            }
            else
            {
                string nombre = getContenido();
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada" + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    match(clasificaciones.identificador); // Validar existencia
                }

                match(clasificaciones.asignacion);

                string valor;

                // Requerimiento 2
                if (getClasificacion() == clasificaciones.cadena)
                {
                    valor = getContenido();
                    if (l.getTipoDato(nombre) == Variable.tipo.STRING)
                    {
                        valor = getContenido();
                        if (ejecuta)
                        {
                            l.setValor(nombre, valor);
                        }
                        match(clasificaciones.cadena);
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico4: No se puede asignar un STRING a un (" + l.getTipoDato(nombre) + ")" + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    // Requerimiento 3
                    MaxBytes = Variable.tipo.CHAR;
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();

                    if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
                    {
                        MaxBytes = tipoDatoExpresion(float.Parse(valor));
                    }

                    if (MaxBytes > l.getTipoDato(nombre))
                    {
                        throw new Error(bitacora, "Error semantico5: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + "(" + linea + ", " + caracter + ")");

                    }
                }
                if (ejecuta)
                {
                    l.setValor(nombre, valor);
                }
                match(clasificaciones.finSentencia);
            }
        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {

            match("const");
            string tipoDato = getContenido();
            match(clasificaciones.tipoDato);

            Variable.tipo Tipo;

            switch (tipoDato)
            {
                case "char":
                    Tipo = Variable.tipo.CHAR;
                    break;

                case "float":
                    Tipo = Variable.tipo.FLOAT;
                    break;

                case "int":
                    Tipo = Variable.tipo.INT;
                    break;

                case "string":
                    Tipo = Variable.tipo.STRING;
                    break;

                default:
                    Tipo = Variable.tipo.CHAR;
                    break;
            }
            string nombre = getContenido();
            if (!l.Existe(nombre) && ejecuta)
            {
                match(clasificaciones.identificador); // Validar duplicidad
                l.Inserta(nombre, Tipo, true);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis:La constante (" + nombre + ") está duplicada" + "(" + linea + ", " + caracter + ")");
            }

            match(clasificaciones.asignacion);

            string valor = getContenido();
            if (getClasificacion() == clasificaciones.numero)
            {
                match(clasificaciones.numero);
                if (ejecuta)
                {
                    l.setValor(nombre, valor);
                }
            }
            else
            {
                match(clasificaciones.cadena);
                if (ejecuta)
                {
                    l.setValor(nombre, valor);
                }
            }

            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if (ejecuta)
                {
                    Console.Write(getContenido());
                    match(clasificaciones.numero);
                }
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {

                string cadena = getContenido();
                if (cadena.Contains("\""))
                {
                    cadena = cadena.Replace("\"", "");
                }
                if (cadena.Contains("\\n"))
                {
                    cadena = cadena.Replace("\\n", "");
                    Console.Write("\n");
                }
                if (cadena.Contains("\\t"))
                {
                    cadena = cadena.Replace("\\t", "");
                    Console.Write("\t");
                }

                if (ejecuta)
                {
                    Console.Write(cadena);
                }
                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada" + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    if (ejecuta)
                    {
                        Console.Write(l.getValor(nombre));
                    }
                    match(clasificaciones.identificador); // Validar existencia

                }
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If(bool ejecuta2)
        {
            bool ejecuta;
            match("if");
            match("(");
            if (getContenido() == "!")
            {
                match(clasificaciones.operadorLogico);
                match("(");
                ejecuta = !Condicion();
                match(")");
            }
            else
            {
                ejecuta = Condicion();
            }
            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(!ejecuta && ejecuta2);
            }
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            MaxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            MaxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);

            switch (operador)
            {
                case ">":
                    return n1 > n2;
                case ">=":
                    return n1 >= n2;
                case "<":
                    return n1 < n2;
                case "<=":
                    return n1 <= n2;
                case "==":
                    return n1 == n2;
                case "!=":
                case "<>":
                    return n1 != n2;

                default:
                    return n1 != n2;
            }
        }

        // x26 = (3+5)*8-(10-4)/2;
        // Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                string operador = getContenido();
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);
                // Console.Write(operador + " ");

                switch (operador)
                {
                    case "+":
                        s.push(e2 + e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2 - e1, bitacora, linea, caracter);
                        break;
                }

                s.display(bitacora);
            }
        }
        // Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                string operador = getContenido();
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);
                // Console.Write(operador + " ");

                switch (operador)
                {
                    case "*":
                        s.push(e2 * e1, bitacora, linea, caracter);
                        break;
                    case "/":
                        s.push(e2 / e1, bitacora, linea, caracter);
                        break;
                }

                s.display(bitacora);
            }
        }
        // Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                string nombre = getContenido();
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada" + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    s.push(float.Parse(l.getValor(getContenido())), bitacora, linea, caracter);
                    s.display(bitacora);
                    match(clasificaciones.identificador); // Validar existencia

                    if (l.getTipoDato(nombre) > MaxBytes)
                    {
                        MaxBytes = l.getTipoDato(nombre);
                    }
                }
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora);

                if (tipoDatoExpresion(float.Parse(getContenido())) > MaxBytes)
                {
                    MaxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                }
                match(clasificaciones.numero);
            }
            else
            {
                match("(");

                bool huboCast = false;
                Variable.tipo tipoDato = Variable.tipo.CHAR;
                if (getClasificacion() == clasificaciones.tipoDato)
                {
                    huboCast = true;
                    tipoDato = determinarTipoDato(getContenido());
                    match(clasificaciones.tipoDato);
                    match(")");
                    match("(");
                }
                Expresion();
                match(")");

                if (huboCast)
                {
                    //Hacer un pop y convertir ese número al tipo dato y meterlo al stack
                    float n1 = s.pop(bitacora, linea, caracter);
                    n1 = casteo(tipoDato, n1);
                    s.push(n1, bitacora, linea, caracter);
                    MaxBytes = tipoDato;
                }
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For(bool ejecuta)
        {
            match("for");

            match("(");

            string nombre = getContenido();
            if (!l.Existe(nombre))
            {
                throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada" + "(" + linea + ", " + caracter + ")");
            }
            else
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);

            Condicion();
            match(clasificaciones.finSentencia);

            string nombre2 = getContenido();
            if (!l.Existe(nombre2))
            {
                throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre2 + ") no está declarada" + "(" + linea + ", " + caracter + ")");
            }
            else
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            match(clasificaciones.incrementoTermino);

            match(")");

            BloqueInstrucciones(ejecuta);
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones(ejecuta);
        }

        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile(bool ejecuta)
        {
            match("do");

            BloqueInstrucciones(ejecuta);

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        private Variable.tipo tipoDatoExpresion(float valor)
        {
            if (valor % 1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if (valor < 256)
            {
                return Variable.tipo.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.tipo.INT;
            }

            return Variable.tipo.FLOAT;
        }

        private Variable.tipo determinarTipoDato(string tipoDato)
        {
            Variable.tipo tipoVar;

            switch (tipoDato)
            {
                case "int":
                    tipoVar = Variable.tipo.INT;
                    break;

                case "float":
                    tipoVar = Variable.tipo.FLOAT;
                    break;

                case "string":
                    tipoVar = Variable.tipo.STRING;
                    break;

                default:
                    tipoVar = Variable.tipo.CHAR;
                    break;
            }

            return tipoVar;
        }

        private float casteo(Variable.tipo TipoDato, float n1)
        {
            float Resultado;
            switch (tipoDatoExpresion(n1))
            {
                case Variable.tipo.INT:
                    if (TipoDato == Variable.tipo.CHAR)
                    {
                        Resultado = n1 % 256;
                        return Resultado;
                    }
                    else
                    {
                        return n1;
                    }

                case Variable.tipo.FLOAT:
                    if (TipoDato == Variable.tipo.FLOAT)
                    {
                        return n1;
                    }
                    else if (TipoDato == Variable.tipo.INT)
                    {
                        Resultado = (int)Math.Round(n1);
                        Resultado = Resultado % 65536;
                        return Resultado;
                    }
                    else if (TipoDato == Variable.tipo.CHAR)
                    {
                        Resultado = (char)Math.Round(n1);
                        Resultado = Resultado % 65536;
                        Resultado = Resultado % 256;
                        return Resultado;
                    }
                    else
                    {
                        Resultado = (int)Math.Round(n1);
                        return Resultado;
                    }
                default:
                    return n1;
            }
        }
    }
}