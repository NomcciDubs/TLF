using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class inputFieldScript : MonoBehaviour
{
    public TMP_InputField inputField;

    private void Start()
    {
        // Agrega un listener para el evento "EndEdit" del InputField
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        // Reemplaza el salto de línea por un retorno de carro al final del texto
        inputField.text += "\n";
        inputField.caretPosition = inputField.text.Length;
        // Activa el InputField nuevamente para permitir la edición continua
        inputField.ActivateInputField();
        inputField.MoveTextEnd(false);
    }
}
