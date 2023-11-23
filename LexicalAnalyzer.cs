using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Text;
using UnityEngine.UI;

//Clase con la logica que sera llamada cada vez que se inserte texto en el inputfield
public class LexicalAnalyzer : MonoBehaviour
{
    private List<Linea> lineas = new List<Linea>();
    private int order = 1;

    public TMP_InputField inputField; // Asigna el InputField en el Inspector
    public TextMeshProUGUI formattedText;
    public TMP_Text tmpTextBehind;
    public TMP_Text tmpTextRecorrido;
    public Button boton;
    public GameObject panel;

    private void Start()
    {
        boton.onClick.AddListener(IniciarRecorrido);
        if (inputField != null)
        {
            // Agrega un listener para el evento "onValueChanged" del TMP_InputField
            inputField.onValueChanged.AddListener(OnInputValueChanged);
        }

        if (tmpTextBehind != null)
        {
            // Configura el TMP_Text detrás para reflejar el texto del InputField
            tmpTextBehind.text = inputField.text;
        }

        else
        {
            Debug.LogError("InputField no asignado en el Inspector. Asigna el InputField en el Inspector.");
        }
        
    }

    private void OnInputValueChanged(string newText)
    {
        
        // Desvincular el evento temporalmente para evitar ciclos infinitos
        inputField.onValueChanged.RemoveListener(OnInputValueChanged);

        // Ejecuta el análisis léxico
        AnalyzeInput(newText);
        string formattedText = FormatTextWithColor(newText);
        tmpTextBehind.text = formattedText;
        
        Debug.Log($"    El texto es {tmpTextBehind.text} ");
        Debug.Log($"y tiene de tamaño {tmpTextBehind.text.Length}");
        // Volver a vincular el evento después de aplicar el formato
        inputField.onValueChanged.AddListener(OnInputValueChanged);

        
    }

    private void AnalyzeInput(string newText)
    {
        lineas.Clear(); // Limpiar la lista antes de un nuevo análisis
        order = 1;

        int currentIndex = 0;
        int numeroLinea = 0;
        List<TokenInfo> tokensEnLinea = new List<TokenInfo>();
        while (currentIndex < newText.Length)
        {
            // Identificar cambios de línea y comentarios
            if (newText[currentIndex] == ';')
            {
            // Es un token terminal (punto y coma)
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.Terminal, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));

            // Incrementa el contador de órdenes
            order++;
            }
            else if (newText[currentIndex] == '\n' || newText[currentIndex] == ';')
            {
                if(newText[currentIndex-1] != ';'){
                    numeroLinea++;
                }
            }
            else if (newText[currentIndex] == '%')
            {
                //Encontro un comentario entonces busca el final de la línea
                int startComment = currentIndex;
                int endComment = newText.IndexOf('\n', currentIndex);

                if (endComment == -1)
                {
                    // Si no se encuentra un cambio de línea, el comentario llega hasta el final del texto
                    endComment = newText.Length;
                }
                // Agrega el comentario a la lista de líneas
                lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                {
                    new TokenInfo(TokenType.ComentarioLinea, Enumerable.Range(startComment, endComment - startComment).ToArray(), order)
                }));

                // Incrementa el contador de órdenes
                order++;
                // Continua el análisis después del comentario
                currentIndex = endComment;
                
                continue;
            }
            else if (char.IsDigit(newText[currentIndex]))
            {
                // Inicio de un número
                int startNumber = currentIndex;

                // Buscar el final del número
                while (currentIndex < newText.Length && (char.IsDigit(newText[currentIndex]) || newText[currentIndex] == '.'))
                {
                    currentIndex++;
                }

                // Fin del número
                int endNumber = currentIndex;

                // Verificar si es un número entero o real
                string numberString = newText.Substring(startNumber, endNumber - startNumber);
                if (numberString.Contains('.'))
                {
                    // Es un número real
                    lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                    {
                        new TokenInfo(TokenType.NumeroReal, Enumerable.Range(startNumber, endNumber - startNumber).ToArray(), order)
                    }));
                }
                else
                {
                    // Es un número entero
                    lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                    {
                        new TokenInfo(TokenType.NumeroNatural, Enumerable.Range(startNumber, endNumber - startNumber).ToArray(), order)
                    }));
                }

                // Incrementa el contador de órdenes
                order++;

                continue;
            }
            else if ((currentIndex + 1 < newText.Length && newText.Substring(currentIndex, 2) == "if") ||
                (currentIndex + 5 < newText.Length && newText.Substring(currentIndex, 6) == "elseif") ||
                (currentIndex + 5 < newText.Length && newText.Substring(currentIndex, 6) == "switch") ||
                (currentIndex + 3 < newText.Length && newText.Substring(currentIndex, 4) == "case") ||
                (currentIndex + 2 < newText.Length && newText.Substring(currentIndex, 3) == "for") ||
                (currentIndex + 4 < newText.Length && newText.Substring(currentIndex, 5) == "while"))
            {
                // Es una palabra reservada "if" o "elseif"
                int startReservada = currentIndex;

                // Buscar el final de la palabra reservada
                while (currentIndex < newText.Length && char.IsLetter(newText[currentIndex]))
                {
                    currentIndex++;
                }

                // Obtener el índice del último carácter de la palabra reservada
                int endReservada = currentIndex - 1;

                lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                {
                    new TokenInfo(TokenType.PalabraReservada, Enumerable.Range(startReservada, endReservada - startReservada + 1).ToArray(), order)
                }));

                // Incrementa el contador de órdenes
                order++;

                continue;
            }
            else if (newText[currentIndex] == '\'')
            {
                int startCadena = currentIndex;

                // Buscar el final de la cadena
                currentIndex++;
                while (currentIndex < newText.Length && newText[currentIndex] != '\'')
                {
                    currentIndex++;
                }

                // Verificar si se encontró el final de la cadena
                if (currentIndex < newText.Length && newText[currentIndex] == '\'')
                {
                    int endCadena = currentIndex;

                    // Agregar la cadena a la lista de líneas
                    lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                    {
                        new TokenInfo(TokenType.CadenaCaracteres, Enumerable.Range(startCadena, endCadena - startCadena + 1).ToArray(), order)
                    }));

                    // Incrementa el contador de órdenes
                    order++;

                    // Continúa después de la cadena
                    currentIndex++;
                    continue;
                }
            }

            else if (newText[currentIndex] == '(' || newText[currentIndex] == ')')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.Parentesis, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }
            else if (currentIndex + 1 < newText.Length &&
                ((newText[currentIndex] == '&' && newText[currentIndex + 1] == '&') ||
                (newText[currentIndex] == '|' && newText[currentIndex + 1] == '|')))
            {
                // Es un operador lógico && o ||
                int startOperador = currentIndex;
                int endOperador = currentIndex + 1;

                // Agregar el operador lógico a la lista de líneas
                lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                {
                    new TokenInfo(TokenType.OperadorLogico, Enumerable.Range(startOperador, endOperador - startOperador + 1).ToArray(), order)
                }));

                // Incrementa el contador de órdenes
                order++;

                // Continúa después del operador lógico
                currentIndex = endOperador + 1;
                continue;
            }
            else if (newText[currentIndex] == '+' || newText[currentIndex] == '-')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.OperadorIncrementoDecremento, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }
            else if (newText[currentIndex] == '*'|| newText[currentIndex] == '/')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.OperadorAritmetico, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }
            else if (currentIndex + 1 < newText.Length &&
                ((newText[currentIndex] == '=' && newText[currentIndex + 1] == '=') ||
                (newText[currentIndex] == '<' && newText[currentIndex + 1] == '=') ||
                (newText[currentIndex] == '>' && newText[currentIndex + 1] == '=')||
                (newText[currentIndex] == '!' && newText[currentIndex + 1] == '=') ))
            {
                // Es un operador lógico && o ||
                int startOperador = currentIndex;
                int endOperador = currentIndex + 1;

                // Agregar el operador lógico a la lista de líneas
                lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                {
                    new TokenInfo(TokenType.OperadorComparacion, Enumerable.Range(startOperador, endOperador - startOperador + 1).ToArray(), order)
                }));

                // Incrementa el contador de órdenes
                order++;

                // Continúa después del operador lógico
                currentIndex = endOperador + 1;
                continue;
            }
            else if (newText[currentIndex] == '=')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.OperadorAsignacion, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }
            else if (newText[currentIndex] == '<' || newText[currentIndex] == '>')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.OperadorComparacion, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }
            else if (newText[currentIndex] == ',')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.Separador, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }
            else if (newText[currentIndex] == '{' || newText[currentIndex] == '}')
            {
            lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.Llaves, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }

            else if (currentIndex < newText.Length && char.IsLetter(newText[currentIndex]))
            {
                // Es un posible identificador
                int startIdentificador = currentIndex;

                // Buscar el final del identificador
                while (currentIndex < newText.Length &&
                    (char.IsLetterOrDigit(newText[currentIndex]) || newText[currentIndex] == '_'))
                {
                    currentIndex++;
                }

                // Obtener el índice del último carácter del identificador
                int endIdentificador = currentIndex - 1;

                // Agregar el identificador a la lista de líneas
                lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
                {
                    new TokenInfo(TokenType.Identificador, Enumerable.Range(startIdentificador, endIdentificador - startIdentificador + 1).ToArray(), order)
                }));

                // Incrementa el contador de órdenes
                order++;

                continue;
            }
            else if(newText[currentIndex] != ' '){
                lineas.Add(new Linea(numeroLinea, new List<TokenInfo>
            {
                new TokenInfo(TokenType.Error, Enumerable.Range(currentIndex, 1).ToArray(), order)
            }));
            order++;
            }



        currentIndex++;
        }
    }

    private void OnEnable()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnInputValueChanged);
        }
        else
        {
            Debug.LogError("TMP_InputField no asignado en el Inspector. Asigna el TMP_InputField en el Inspector.");
        }
    }

    private void OnDisable()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputValueChanged);
        }
    }
    private string FormatTextWithColor(string originalText)
    {
        StringBuilder formattedText = new StringBuilder(originalText);
        int tamanioAcumulado = 0;

        foreach (var linea in lineas)
        {
            foreach (var tokenInfo in linea.TokensEnLinea)
            {
                // Obtiene el color correspondiente a la categoría del token
                string color = GetColorForTokenType(tokenInfo.TokenType);

                // Calcula el tamaño de las etiquetas de apertura y cierre de color
                int colorTagLength = $"<color={color}></color>".Length;
                int colorTagLengthI = $"<color={color}>".Length;
                ApplyColorToLine(formattedText, tokenInfo.CharIndexes[0]+tamanioAcumulado, tokenInfo.CharIndexes.Last()+1+tamanioAcumulado, color);

                // Añade el tamaño del tag de apertura y cierre de color al tamaño acumulado
                tamanioAcumulado += colorTagLength;
                
            }
        }

        return formattedText.ToString();
    }

    private string GetColorForTokenType(TokenType tokenType)
    {
        // Devuelve el color correspondiente a la categoría del token
        switch (tokenType)
        {
            case TokenType.ComentarioLinea:
                return "green";
            case TokenType.NumeroNatural:
                return "#B0B0B0"; 
            case TokenType.NumeroReal:
                return "#808080"; 
            case TokenType.Terminal:
                return "yellow"; 
            case TokenType.CadenaCaracteres:
                return "yellow";
            case TokenType.OperadorAsignacion:
                return "yellow";
            case TokenType.OperadorAritmetico:
                return "yellow";
            case TokenType.OperadorIncrementoDecremento:
                return "yellow";
            case TokenType.Parentesis:
                return "blue";
            case TokenType.Llaves:
                return "blue";
            case TokenType.OperadorLogico:
                return "blue";
            case TokenType.Separador:
                return "blue";
            case TokenType.OperadorComparacion:
                return "blue";
            case TokenType.Identificador:
                return "#00FFFF";
            case TokenType.Error:
                return "red";
            case TokenType.PalabraReservada:
                return "purple"; 
            default:
                return "white"; // Color predeterminado
        }
    }

    private void ApplyColorToLine(StringBuilder text, int startIndex, int endIndex, string color)
    {
        // Inserta la etiqueta de apertura de color al principio del comentario
        text.Insert(startIndex, $"<color={color}>");

        // Inserta la etiqueta de cierre de color al final del comentario
        text.Insert(endIndex + $"<color={color}>".Length, $"</color>");
    }

    private void UpdateInputFieldText(string newText)
    {
        if (inputField != null)
        {
            inputField.text = newText;
        }
        else
        {
            Debug.LogError("TMP_InputField no asignado en el Inspector. Asigna el TMP_InputField en el Inspector.");
        }
    }

    void IniciarRecorrido()
    {
        panel.SetActive(true);
        tmpTextRecorrido.text = "";
        string texto = "Empieza en: ";
        
        for (int i = 0; i < lineas.Count; i++)
        {
            Linea linea = lineas[i];
            int totalTokens = linea.TokensEnLinea.Count;

            for (int j = 0; j < linea.TokensEnLinea.Count; j++)
            {
                TokenInfo token = linea.TokensEnLinea[j];
                
                // Añade " >> " solo si no es el último token de la línea y no es el último token de la última línea
                if (j != linea.TokensEnLinea.Count - 1 || i != lineas.Count - 1)
                {
                    texto += token.TokenType + " >> ";
                }
                else
                {
                    texto += token.TokenType;
                }
            }
        }

        tmpTextRecorrido.text = texto;
    }

}