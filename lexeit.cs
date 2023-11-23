using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
//Clase enum para crear las categorias de cada Token que se usara a lo largo del codigo
public enum TokenType
{
    NumeroNatural,
    NumeroReal,
    Identificador,
    PalabraReservada,
    OperadorAritmetico,
    OperadorComparacion,
    OperadorLogico,
    OperadorAsignacion,
    OperadorIncrementoDecremento,
    Parentesis,
    Llaves,
    Terminal,
    Separador,
    Hexadecimal,
    CadenaCaracteres,
    ComentarioLinea,
    ComentarioBloque,
    Error
}
//Clase linea para separar los tokens por lineas
public class Linea
{
    public int NumeroLinea { get; set; }
    public List<TokenInfo> TokensEnLinea { get; set; }
    public int TamanoLinea { get; set; }

    public Linea(int numeroLinea, List<TokenInfo> tokensEnLinea)
    {
        NumeroLinea = numeroLinea;
        TokensEnLinea = tokensEnLinea;
        CalcularTamanoLinea();
    }

    private void CalcularTamanoLinea()
    {
        TamanoLinea = 0;
        foreach (var tokenInfo in TokensEnLinea)
        {
            if (tokenInfo.CharIndexes.Length > 0)
            {
                TamanoLinea = Mathf.Max(TamanoLinea, tokenInfo.CharIndexes.Max() + 1);
            }
        }
    }
}
//Clase token info de la que sacaremos informacion de cada palabra, El tipo de token, index que se encuentran sus char y su orden en el recorrido.
public class TokenInfo
{
    public TokenType TokenType { get; set; }
    public int[] CharIndexes { get; set; }
    public int Order { get; set; }

    public TokenInfo(TokenType tokenType, int[] charIndexes, int order)
    {
        TokenType = tokenType;
        CharIndexes = charIndexes;
        Order = order;
    }
}
