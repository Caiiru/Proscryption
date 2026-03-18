using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace DeveloperConsole
{
    public class Console : MonoBehaviour
    {
        /**
         * Access this property from your code to access this script.
         * E.g.: Console.Singleton.isSelected
         */
        public static Console Singleton { get; set;}
        
        [Header("Customization")]

        [Tooltip("Which key to use to open the console.")]
        /**
         * Choose on the inspector which key to use to open the console.
         */
        public KeyCode keyOpen = KeyCode.Tilde;
        
        [Tooltip("Which key to use to open the console.")]
        /**
         * Choose on the inspector which key to use to open the console.
         */
        public KeyCode keyOpenAlt = KeyCode.Quote;

        [Tooltip("Which key to use to open the console and immediately start typing, or type onto the already open console without clicking.")]
        /**
         * Choose on the inspector which key to use to open the console and immediately start typing, or type onto the already open console without clicking.
         */
        public KeyCode keyType = KeyCode.T;

        [ColorHtmlProperty] public Color colorDefault = Color.white;
        [ColorHtmlProperty] public Color colorSuccess = Color.green;
        [ColorHtmlProperty] public Color colorWarning = Color.yellow;
        [ColorHtmlProperty] public Color colorError = Color.red;
        [ColorHtmlProperty] public Color colorCommands = Color.magenta;

        [Space(10)]
        [Header("Internal")]
        public TMP_InputField inputField;
        public TMP_Text consoleText;
        public Scrollbar scrollBar;
        public RectTransform consoleTextPanelRect;
        public RectTransform inputFieldRect;
        public RectTransform scrollBarRect;

        [Space(10)]
        public Button resizeBtn;
        public Button closeBtn;

        /**
         * Format your custom commands should use.
         * E.g.: void TeleportPlayerCommand(string[] args)
         */
        public delegate void Command(string[] args);

        Dictionary<string, Command> commands = new Dictionary<string, Command>();

        /**
         * True if the input field is currently selected. Can be used to lock player input while typing.
         */
        public bool isSelected = false;

        EventSystem eventSystem;

        List<string> lastCommands = new List<string>();
        int commandBrowseCount = 0;
        
        string m_colorDefault = "<color=\"white\">";
        string m_colorSuccess = "<color=\"green\">";
        string m_colorWarning = "<color=\"yellow\">";
        string m_colorError = "<color=\"red\">";
        string m_colorCommands = "<color=\"orange\">";
        string m_colorEnd = "</color>";
        bool isOpen = false;
        bool isResizing = false;
        Vector2 resizeInitialMousePosition = Vector2.zero;
        Vector2 resizeInitialOffset = Vector2.zero;
        Vector2 resizeInitialAnchoredPosition = Vector2.zero;

        /**
         * @brief Use this method to add new commands the console.
         *
         * @param commandText The string that will have to be submitted via the input field to use this command.
         * @param command The method that will be called, with 'string[] args' as argument
         *
         * @return True if successful.
         */
        public static bool AddCommand(string commandText, Command command)
        {
            return Singleton.M_AddCommand(commandText, command);
        }

        /**
         * @brief Use this method to remove commands from the console.
         *
         * @param commandText The command to remove from the list.
         *
         * @return True if successful.
         */
        public static bool RemoveCommand(string commandText)
        {
            return Singleton.M_RemoveCommand(commandText);
        }

        /**
         * @brief Prints default colored message to the console.
         *
         * @param message Message to print.
         */
        public static void Print(string message)
        {
            Singleton.M_Print(message);
        }

        /**
         * @brief Prints sucess colored message to the console.
         *
         * @param message Message to print.
         */
        public static void PrintSuccess(string message)
        {     
           Singleton.M_PrintSuccess(message);
        }

        /**
         * @brief Prints warning colored message to the console.
         *
         * @param message Message to print.
         */
        public static void PrintWarning(string message)
        {     
            Singleton.M_PrintWarning(message);
        }

        /**
         * @brief Prints error colored message to the console.
         *
         * @param message Message to print.
         */
        public void PrintError(string message)
        {     
            Singleton.M_PrintError(message);
        }

        /**
         * @brief Use to open console from another script.
         */
        public static void Open()
        {
            Singleton.M_Open();
        }

        /**
         * @brief Use to close console from another script.
         */
        public static void Close()
        {
            Singleton.M_Close();
        }

        /**
         * @brief Use to toggle console from another script.
         */
        public void Toggle()
        {
            Singleton.M_Toggle();
        }





        // -----------------------------------------------------------------------------------------------------
        // -----------------------------------------------------------------------------------------------------

        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }

            eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

            m_colorDefault = "<color=#" + ColorUtility.ToHtmlStringRGB(colorDefault) + ">";
            m_colorSuccess = "<color=#" + ColorUtility.ToHtmlStringRGB(colorSuccess) + ">";
            m_colorWarning = "<color=#" + ColorUtility.ToHtmlStringRGB(colorWarning) + ">";
            m_colorError   = "<color=#" + ColorUtility.ToHtmlStringRGB(colorError  ) + ">";
            m_colorCommands = "<color=#" + ColorUtility.ToHtmlStringRGB(colorCommands) + ">";

            AddCommand("clear", ClearLogCommand);
            AddCommand("help", HelpCommand);
        }

        void Start()
        {
            inputField.onSelect.AddListener((string word) => {
                isSelected = true;
            });
            inputField.onDeselect.AddListener((string word) => {
                isSelected = false;
            });
            inputField.onEndEdit.AddListener((string word) => {
                //print("End edit");
            });

            Close();
        }
        
        void Update()
        {
            ProcessInput();
        }

        IEnumerator UpdateScrollbar()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            scrollBar.value = 0.0f;
        }

        void ProcessInput()
        {
            if (isSelected)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    AutoComplete();
                    
                }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (inputField.text.Length > 0)
                    {
                        ExecuteCommand(inputField.text);
                        inputField.text = "";
                        
                    }
                    eventSystem.SetSelectedGameObject(null);
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    commandBrowseCount++;
                    commandBrowseCount = System.Math.Min(commandBrowseCount, lastCommands.Count);
                    if (lastCommands.Count > 0)
                    {
                        inputField.text = lastCommands[commandBrowseCount-1];
                    }
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    commandBrowseCount--;
                    commandBrowseCount = System.Math.Max(commandBrowseCount, 0);
                    inputField.text = commandBrowseCount > 0 ? lastCommands[commandBrowseCount-1] : "";
                }
            }
            else
            {
                if (Input.GetKeyDown(keyOpen) || Input.GetKeyDown(keyOpenAlt))
                {
                    Toggle();
                }
                if (Input.GetKeyDown(keyType) && !isOpen)
                {
                    Open();
                }
            }

            if (Input.GetKeyDown(keyType))
            {
                inputField.Select();
            }

            if (resizeBtn.GetComponent<LongPress>().buttonPressed && !isResizing)
            {
                StartResizing();
            }
            if (Input.GetMouseButtonUp(0) && isResizing)
            {
                StopResizing();
            }
        }

        protected void M_Print(string message)
        {
            consoleText.text += m_colorDefault + message + "\n";
            
            StartCoroutine(UpdateScrollbar());
        }

        protected void M_PrintSuccess(string message)
        {     
            consoleText.text += m_colorSuccess + message + m_colorSuccess + "\n";
            
            StartCoroutine(UpdateScrollbar());
        }

        protected void M_PrintWarning(string message)
        {     
            consoleText.text += m_colorWarning + message + m_colorEnd + "\n";
            
            StartCoroutine(UpdateScrollbar());
        }

        protected void M_PrintError(string message)
        {     
            consoleText.text += m_colorError + message + m_colorEnd + "\n";
            
            StartCoroutine(UpdateScrollbar());
        }

        protected bool M_AddCommand(string commandText, Command command){

            bool exists = false;

            foreach (KeyValuePair<string, Command> kvp in commands)
            {
                if (string.Equals(kvp.Key, commandText))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                commands.Add(commandText, command);
                return true;
            }
            PrintWarning(string.Format("Command '{0}{1}{2}' already added", m_colorCommands, commandText, m_colorEnd));
            return false;
        }


        protected bool M_RemoveCommand(string commandText)
        {
            foreach (KeyValuePair<string, Command> kvp in commands)
            {
                if (string.Equals(kvp.Key, commandText))
                {
                    commands.Remove(kvp.Key);
                    return true;
                }
            }

            return false;
        }

        void ExecuteCommand(string commandText)
        {
            lastCommands.Insert(0, commandText);

            string[] args = commandText.Split(' ');
            bool foundCommand = false;
            foreach (KeyValuePair<string, Command> kvp in commands)
            {
                if (string.Equals(args[0], kvp.Key))
                {
                    Print(string.Format("{0}Executing command '{1}{2}{3}{4}'", m_colorDefault, m_colorCommands, commandText, m_colorEnd, m_colorDefault));
                    try
                    {
                        kvp.Value(args.Skip(1).ToArray());
                    }
                    catch (System.Exception e)
                    {
                        PrintError(e.Message);
                    }
                    foundCommand = true;
                    break;
                }
            }
            if (!foundCommand)
            {
                Print(string.Format("{0}Command '{1}{2}{3}{4}' not found. Type \'help\' for a list of available commands{5}", 
                    m_colorError, m_colorCommands, commandText, m_colorEnd, m_colorError, m_colorEnd));
            }

            commandBrowseCount = 0;
        }

        void AutoComplete()
        {
            List<string> possibilities = new List<string>();
            foreach (KeyValuePair<string, Command> kvp in commands)
            {
                if (kvp.Key.StartsWith(inputField.text) && !string.Equals(kvp.Key, inputField.text))
                {
                    possibilities.Add(kvp.Key);
                }
            }
            
            if (possibilities.Count > 0)
            {
                string possibilitiesLine = m_colorCommands;
                foreach (string possibility in possibilities)
                {
                    possibilitiesLine += possibility + " ";
                }
                possibilitiesLine += m_colorEnd;

                if (possibilities.Count == 1)
                {
                    inputField.text = possibilities[0];
                    inputField.caretPosition = inputField.text.Length;
                }
                else
                {
                    Print(possibilitiesLine);
                }
            }
        }

        void ClearLogCommand(string[] args)
        {
            consoleText.text = string.Empty;
        }

        void HelpCommand(string[] args)
        {
            Print("Available commands:");

            string commandList = m_colorCommands;
            foreach (KeyValuePair<string, Command> kvp in commands)
            {
                commandList += kvp.Key + "  ";
            }

            commandList += m_colorEnd;

            Print(commandList);
        }

        protected void M_Open()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            isOpen = true;
            inputField.Select();
        }

        protected void M_Close()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            isOpen = false;
        }

        protected void M_Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        void StartResizing()
        {
            resizeInitialOffset = consoleTextPanelRect.sizeDelta;
            resizeInitialMousePosition = Input.mousePosition;
            resizeInitialAnchoredPosition = consoleTextPanelRect.anchoredPosition;

            isResizing = true;

            StartCoroutine(Resize());
        }

        void StopResizing()
        {
            isResizing = false;
            scrollBar.value = 0f;
        }

        IEnumerator Resize()
        {
            while (true)
            {
                if (isResizing)
                {
                    Vector2 mouseOffset = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - resizeInitialMousePosition;
                    Vector2 newSizeDelta = resizeInitialOffset + mouseOffset;
                    Vector2 newSizeDeltaClamped = new Vector2(-newSizeDelta.x > -380f ? 380f : newSizeDelta.x, Mathf.Max(newSizeDelta.y, 150.0f));

                    consoleTextPanelRect.sizeDelta = newSizeDeltaClamped;
                    SetLeft(consoleTextPanelRect, 0f);

                    inputFieldRect.sizeDelta = new Vector2(newSizeDeltaClamped.x, inputFieldRect.sizeDelta.y);
                    SetLeft(inputFieldRect, 0f);

                    scrollBarRect.sizeDelta = new Vector2(10f, newSizeDeltaClamped.y - 24);
                    scrollBarRect.anchoredPosition = new Vector2(newSizeDeltaClamped.x, scrollBarRect.anchoredPosition.y);
                }
                else
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        // credit: Eldoir https://answers.unity.com/questions/888257/access-left-right-top-and-bottom-of-recttransform.html
        void SetLeft(RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }
        void SetRight(RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
        void SetTop(RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
        void SetBottom(RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
    }
}
