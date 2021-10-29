using System;
using System.Collections.Generic;
using System.Text;

// ✓  Requerimiento 1: Programar el residuo de la division en PorFactor
//                    para C++ y ensamblador y hacer un modo de linea cuando
//                    se imprima un "\n".
// ✓  Requerimiento 2: Programar (en ensamblador) el else. Nota: será necesario 
//                    agregar etiquetas para el else.
//    Requerimiento 3: Agregar la negación de la condición.
// ✓  Requerimiento 4: Declarar Variables en el For (int i).
// ✓  Requerimiento 5: Actualizar la variable del for con "+=" y "-=".
// ✓ 

namespace Automatas
{
    class Lenguaje : Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo MaxBytes;
        int numeroIf;
        int numeroFor;
        int numeroElse;
        public Lenguaje()
        {
            s = new Stack(5);
            l = new ListaVariables();
            numeroIf = numeroFor = numeroElse = 0;
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre) : base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            numeroIf = numeroFor = numeroElse = 0;
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            asm.WriteLine("include \"emu8086.inc\"");
            asm.WriteLine("org 100h");
            Libreria();
            Main();
            asm.WriteLine("ret");
            asm.WriteLine("define_print_string");
            asm.WriteLine("define_print_num");
            asm.WriteLine("define_print_num_uns");
            asm.WriteLine("define_scan_num");
            asm.WriteLine("; variables");
            l.imprime(bitacora, asm);
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
                throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") está duplicada " + "(" + linea + ", " + caracter + ")");
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
                        throw new Error(bitacora, "Error semantico: No se puede asignar un STRING a un " + TIPO + " (" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //Requerimiento 3
                    Expresion();
                    MaxBytes = Variable.tipo.CHAR;

                    string valor;
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    asm.WriteLine("\tPOP CX");

                    if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
                    {
                        MaxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if (MaxBytes > TIPO)
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + " (" + linea + ", " + caracter + ")");
                    }

                    asm.WriteLine("\tMOV " + nombre + ", CX");

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
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada" + " (" + linea + ", " + caracter + ")");
                }
                else
                {
                    asm.WriteLine("\tcall scan_num");
                    asm.WriteLine("\tMOV " + nombre + ", CX");
                    asm.WriteLine("\tprintn \"\"");
                    if (ejecuta)
                    {
                        match(clasificaciones.identificador);
                        string valor = Console.ReadLine();

                        if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
                        {
                            MaxBytes = tipoDatoExpresion(float.Parse(valor));
                        }

                        if (MaxBytes > l.getTipoDato(nombre))
                        {
                            throw new Error(bitacora, "Error semantico: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + " (" + linea + ", " + caracter + ")");
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
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada " + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    match(clasificaciones.identificador);
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
                        throw new Error(bitacora, "Error semantico: No se puede asignar un STRING a un " + l.getTipoDato(nombre) + " (" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    // Requerimiento 3
                    MaxBytes = Variable.tipo.CHAR;
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    asm.WriteLine("\tPOP CX");

                    if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
                    {
                        MaxBytes = tipoDatoExpresion(float.Parse(valor));
                    }

                    if (MaxBytes > l.getTipoDato(nombre))
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + " (" + linea + ", " + caracter + ")");
                    }
                }

                asm.WriteLine("\tMOV " + nombre + ", CX");

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
                match(clasificaciones.identificador);
                l.Inserta(nombre, Tipo, true);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis:La constante (" + nombre + ") está duplicada " + "(" + linea + ", " + caracter + ")");
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
                asm.WriteLine("\t MOV AX, " + getContenido());
                asm.WriteLine("\t call print_num");

                if (ejecuta)
                {
                    Console.Write(getContenido());
                    match(clasificaciones.numero);
                }
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {

                string cadena = getContenido();
                string cadena2 = "\tprint " + getContenido();

                if (cadena.Contains("\""))
                {
                    cadena = cadena.Replace("\"", "");
                }
                if (cadena.Contains("\\n"))
                {
                    cadena = cadena.Replace("\\n", "\n");
                    cadena2 = cadena2.Replace("\\n", "\"\n\t printn \"");
                }
                if (cadena.Contains("\\t"))
                {
                    cadena = cadena.Replace("\\t", "\t");
                }
                asm.WriteLine(cadena2);
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
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada " + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    asm.WriteLine("\t MOV AX, " + nombre);
                    asm.WriteLine("\t call print_num");

                    if (ejecuta)
                    {
                        Console.Write(l.getValor(nombre));
                    }
                    match(clasificaciones.identificador);
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
            bool ejecuta, negacion = true;
            string etiqueta = "if" + numeroIf++;

            match("if");
            match("(");
            if (getContenido() == "!")
            {
                match(clasificaciones.operadorLogico);
                match("(");
                ejecuta = Condicion(etiqueta, negacion);
                match(")");
            }
            else
            {
                ejecuta = Condicion(etiqueta, !negacion);
            }

            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);

            if (getContenido() == "else")
            {
                match("else");
                string etiqueta2 = "else" + numeroElse++;
                asm.WriteLine("\tJMP " + etiqueta2);
                asm.WriteLine(etiqueta + ":");

                BloqueInstrucciones(!ejecuta && ejecuta2);
                asm.WriteLine(etiqueta2 + ":");
                return;
            }
            asm.WriteLine(etiqueta + ":");
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion(string etiqueta, bool negacion)
        {
            MaxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            asm.WriteLine("\tPOP CX");
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            MaxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);
            asm.WriteLine("\tPOP BX");

            asm.WriteLine("\tCMP CX, BX");


            switch (operador)
            {
                case ">":
                    if (negacion == true)
                    {
                        asm.WriteLine("\tJG " + etiqueta);
                        return n1 <= n2;
                    }
                    else
                    {
                        asm.WriteLine("\tJLE " + etiqueta);
                        return n1 > n2;
                    }
                case ">=":
                    if (negacion == true)
                    {
                        asm.WriteLine("\tJGE " + etiqueta);
                        return n1 < n2;
                    }
                    else
                    {
                        asm.WriteLine("\tJL " + etiqueta);
                        return n1 >= n2;
                    }
                case "<":
                    if (negacion == true)
                    {
                        asm.WriteLine("\tJL " + etiqueta);
                        return n1 >= n2;
                    }
                    else
                    {
                        asm.WriteLine("\tJGE " + etiqueta);
                        return n1 < n2;
                    }
                case "<=":
                    if (negacion == true)
                    {
                        asm.WriteLine("\tJLE " + etiqueta);
                        return n1 > n2;
                    }
                    else
                    {
                        asm.WriteLine("\tJG " + etiqueta);
                        return n1 <= n2;
                    }
                case "==":
                    if (negacion == true)
                    {
                        asm.WriteLine("\tJE " + etiqueta);
                        return n1 != n2;
                    }
                    else
                    {
                        asm.WriteLine("\tJNE " + etiqueta);
                        return n1 == n2;
                    }
                default:
                    if (negacion == true)
                    {
                        asm.WriteLine("\tJNE " + etiqueta);
                        return n1 == n2;
                    }
                    else
                    {
                        asm.WriteLine("\tJE " + etiqueta);
                        return n1 != n2;
                    }
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
                asm.WriteLine("\tPOP BX");
                asm.WriteLine("\tPOP AX");
                switch (operador)
                {
                    case "+":
                        asm.WriteLine("\tADD AX, BX");
                        s.push(e2 + e1, bitacora, linea, caracter);
                        asm.WriteLine("\tPUSH AX");
                        break;
                    case "-":
                        asm.WriteLine("\tSUB AX, BX");
                        s.push(e2 - e1, bitacora, linea, caracter);
                        asm.WriteLine("\tPUSH AX");
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
                asm.WriteLine("\tPOP BX");
                asm.WriteLine("\tPOP AX");

                switch (operador)
                {
                    case "*":
                        asm.WriteLine("\tMUL BX");
                        s.push(e2 * e1, bitacora, linea, caracter);
                        asm.WriteLine("\tPUSH AX");
                        break;
                    case "/":
                        asm.WriteLine("\tDIV BX");
                        s.push(e2 / e1, bitacora, linea, caracter);
                        asm.WriteLine("\tPUSH AX");
                        break;
                    case "%":
                        asm.WriteLine("\tDIV BX");
                        s.push(e2 % e1, bitacora, linea, caracter);
                        asm.WriteLine("\tPUSH DX");
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
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") no está declarada " + "(" + linea + ", " + caracter + ")");
                }
                else
                {
                    s.push(float.Parse(l.getValor(getContenido())), bitacora, linea, caracter);
                    asm.WriteLine("\tMOV AX, " + nombre);
                    asm.WriteLine("\tPUSH AX");
                    s.display(bitacora);
                    match(clasificaciones.identificador);

                    if (l.getTipoDato(nombre) > MaxBytes)
                    {
                        MaxBytes = l.getTipoDato(nombre);
                    }
                }
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                asm.WriteLine("\tMOV AX, " + getContenido());
                asm.WriteLine("\tPUSH AX");
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
                    //Requerimiento 5
                    //Hacer un pop y convertir ese número al tipo dato y meterlo al stack
                    float n1 = s.pop(bitacora, linea, caracter);
                    asm.WriteLine("\tPOP BX");
                    n1 = casteo(tipoDato, n1);
                    s.push(n1, bitacora, linea, caracter);
                    asm.WriteLine("\tMOV AX, " + n1);
                    asm.WriteLine("\tPUSH AX");
                    MaxBytes = tipoDato;
                }
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For(bool ejecuta2)
        {

            match("for");
            match("(");

            string nombre = getContenido();
            bool ejecuta, negacion = true;
            string etiquetaFin = "endFor" + numeroFor;
            string etiquetaInicio = "beginFor" + numeroFor++;

            if (getClasificacion() == clasificaciones.tipoDato)
            {
                string tipo = getContenido();
                match(clasificaciones.tipoDato);
                nombre = getContenido();

                if (!l.Existe(nombre))
                {
                    match(clasificaciones.identificador);
                    l.Inserta(nombre, determinarTipoDato(tipo));
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis:La variable (" + nombre + ") está duplicada " + "(" + linea + ", " + caracter + ")");
                }
            }
            else
            {
                nombre = getContenido();
                match(clasificaciones.identificador);
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis:La variable " + nombre + " no está declarada " + "(" + linea + ", " + caracter + ")");
                }
            }

            match(clasificaciones.asignacion);

            MaxBytes = Variable.tipo.CHAR;
            Expresion();
            string valor = s.pop(bitacora, linea, caracter).ToString();
            asm.WriteLine("\tPOP CX");

            if (tipoDatoExpresion(float.Parse(valor)) > MaxBytes)
            {
                MaxBytes = tipoDatoExpresion(float.Parse(valor));
            }

            if (MaxBytes > l.getTipoDato(nombre))
            {
                throw new Error(bitacora, "Error semantico: No se puede asignar un " + MaxBytes + " a un " + l.getTipoDato(nombre) + " (" + linea + ", " + caracter + ")");
            }

            asm.WriteLine("\tMOV " + nombre + ", CX");
            l.setValor(nombre, valor);

            match(clasificaciones.finSentencia);
            asm.WriteLine(etiquetaInicio + ":");

            if (getContenido() == "!")
            {
                match(clasificaciones.operadorLogico);
                match("(");
                ejecuta = Condicion(etiquetaFin, negacion);
                match(")");
            }
            else
            {
                ejecuta = Condicion(etiquetaFin, !negacion);
            }

            match(clasificaciones.finSentencia);


            string nombre2 = getContenido();
            match(clasificaciones.identificador);

            if (!l.Existe(nombre2))
            {
                throw new Error(bitacora, "Error de sintaxis:La variable " + nombre2 + " no está declarada " + "(" + linea + ", " + caracter + ")");
            }

            string operador = getContenido();
            match(clasificaciones.incrementoTermino);
            string operadores = "";

            if (operador == "++")
            {
                l.setValor(nombre, (float.Parse(l.getValor(nombre)) + 1).ToString());
                operadores = "\tINC " + nombre;
            }
            else if (operador == "--")
            {
                l.setValor(nombre, (float.Parse(l.getValor(nombre)) - 1).ToString());
                operadores = "\tDEC " + nombre;
            }
            else if (operador == "+=")
            {
                string numero = getContenido();
                match(clasificaciones.numero);
                l.setValor(nombre, (float.Parse(l.getValor(nombre)) + float.Parse(numero)).ToString());
                operadores = "\tADD " + nombre + ", " + numero;

            }
            else if (operador == "-=")
            {
                string numero = getContenido();
                match(clasificaciones.numero);
                l.setValor(nombre, (float.Parse(l.getValor(nombre)) - float.Parse(numero)).ToString());
                operadores = "\tSUB " + nombre + ", " + numero;
            }

            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);
            
            asm.WriteLine(operadores);
            asm.WriteLine("\tjmp " + etiquetaInicio);
            asm.WriteLine(etiquetaFin + ":");
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");
            match("(");
            Condicion("", true);
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
            Condicion("", true);
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

        //Para convertir un int a char se divide /256 y el residuo es el resultado del cast 256 = 0, 257 = 1,...
        //Para convertir un float a int se divide /65536 y el residuo es el resultado del cast.
        //Para convertir un float a char se divide /65535 /256  y el residuo es el resultado del cast 256 = 0, 257 = 1,...
        //Para convertir un float a otro redondear el numero para eliminar la parte fraccional 
        //Para convertir a float n1 = n1.
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
