using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;
using AgateLib.InputLib;
namespace AgateDemo
{
    public class Dialog
    {
        public int contentWidth = 0, contentHeight = 1;
        public double renderX = (Demo.mapDisplayWidth / 2) - 6, renderY = (Demo.mapDisplayHeight / 2) - 20;
        public string speaker;
        public List<string> lines = new List<string>();
        public List<DialogItem> options = new List<DialogItem>() ;
        
        public int currentOption = 0;
        public Dialog previousDialog;
        public Dialog(string speaker, List<string> lines, List<DialogItem> options)//, List<LinkedEvent> events, List<LinkedAction> actions)
        {
            //font = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
            foreach (String s in lines)
            {
                if (s.Length > contentWidth)
                    contentWidth = s.Length;
            }
            foreach (DialogItem s in options)
            {
                if (s.text.Length > contentWidth)
                    contentWidth = s.text.Length;
            }
            this.speaker = speaker;
            this.lines = lines;
            this.options = options;

            renderX -= contentWidth * 3;
            setSize();
        }
        public Dialog Clone()
        {
            return new Dialog(this.speaker, this.lines.ToList(), this.options.ToList());
        }

        public void setSize()
        {
            contentHeight = lines.Count + options.Count;
            if (options.Count > 15)
                contentHeight = lines.Count + 15;
            renderY = (Demo.mapDisplayHeight / 2) + 7 - (contentHeight + 4) * 7;
            foreach (String s in lines)
            {
                if (s.Length > contentWidth)
                    contentWidth = s.Length;
            }
            foreach (DialogItem s in options)
            {
                if (s.text.Length > contentWidth)
                    contentWidth = s.text.Length;
            }
            renderX = ((Demo.mapDisplayWidth / 2) - 6) - (contentWidth * 3);
        }
    }



    public class DialogItem
    {
        //public bool enabled;
        public string text;
        public Dialog linksTo;
        public InputEventHandler eventLink;
        public LinkedAction actionLink;
        public DialogItem(string txt, Dialog dialogLink, LinkedEvent evLink)
        {
            //enabled = true;
            text = txt;
            linksTo = dialogLink;
            if (evLink != null)
            {
                eventLink = new InputEventHandler(evLink);
            }
            else
            {
                eventLink = null;
            }
        }
        public DialogItem(string txt, Dialog dialogLink, LinkedEvent evLink, LinkedAction act)
        {
            //enabled = true;
            text = txt;
            linksTo = dialogLink;
            if (evLink != null)
            {
                eventLink = new InputEventHandler(evLink);
            }
            else
            {
                eventLink = null;
            }
            if (act != null)
            {
                actionLink = act;

            }
        }
        public void handleAction(InputEventArgs e)
        {
            if (linksTo != null)
                DialogBrowser.Navigate(linksTo);
            else if (eventLink != null)
            {
                ScreenBrowser.Hide();
                Demo.mode = InputMode.Map;
                Keyboard.KeyDown -= eventLink;
                Keyboard.KeyDown += eventLink;
            }
            if (actionLink != null)
            {
                actionLink();
            }
        }
        public void handleFinish()
        {
            if (linksTo == null && eventLink != null)
            {
                //DialogBrowser.Navigate(DialogBrowser.currentUI.initialDialog);
               // enabled = false;
                Keyboard.KeyDown -= eventLink;
                Demo.mode = InputMode.Menu; //NOTE: switches to SimpleUI
            }
            else if (actionLink != null)
            {
                //DialogBrowser.Navigate(DialogBrowser.currentUI.initialDialog);
                //enabled = true;
                DialogBrowser.Hide();
            }
        }
        public void handleRecall()
        {
            if (linksTo == null && eventLink != null)
            {
               // DialogBrowser.Navigate(DialogBrowser.currentUI.currentDialog.previousDialog);
                //enabled = true;
                Keyboard.KeyDown -= eventLink;
                Demo.mode = InputMode.Menu;
            }
            else if (linksTo != null)
            {
               // enabled = true;
            }
        }
    }





    public static class DialogBrowser
    {
        //        public static Dialog endDialog;
        public static DialogUI currentUI;
       // public static KeyCode confirmKey = KeyCode.Z, backKey = KeyCode.X;
        public static DialogItem dialogItemForFinish = null;
        public static List<DialogItem> dialogItemsForRecall = new List<DialogItem>();
        public static bool isHidden = false;
        //        public static LinkedAction associatedEvent;
        public static void Init()
        {
            //            associatedEvent = menuEventHandler;
            //endDialog = new Dialog("__END_Dialog__", new List<DialogItem>());
            currentUI = DialogUI.InitUI();

        }
        public static void Navigate(Dialog s)
        {
            // currentUI.initialDialog = currentUI.currentDialog.Clone();
            //            s.previousDialog = currentUI.currentDialog.Clone();
            currentUI.currentDialog = s;
        }
        public static void NavigateBackward(Dialog s)
        {
            // currentUI.initialDialog = currentUI.currentDialog.Clone();
            // currentUI.previousDialog = currentUI.currentDialog.Clone();
            currentUI.currentDialog = s.previousDialog;
        }
        public static void Hide()
        {
            isHidden = true;
            Demo.mode = InputMode.None;
        }
        public static void UnHide()
        {
            isHidden = false;
        }
        public static void Show()
        {
            if (isHidden)
                return;
            currentUI.Show();
        }
        public static void Refresh()
        {
            currentUI.currentDialog = currentUI.initialDialog.Clone();
            foreach (DialogItem mi in currentUI.allDialogItems)
            {
                //mi.enabled = true;
            }
            //            return s;
        }

        /*
         
        public static void OnKeyDown_Dialog(InputEventArgs e)
        {
            if (isHidden || Demo.mode != Demo.InputMode.Dialog)
            {
                return;
            }
            else
            {
                if ((e.KeyCode == KeyCode.Up || (Demo.hjkl && e.KeyCode == KeyCode.K)) && currentDialog.options.Count > 1 && currentOption > 0)
                    currentOption--;
                else if ((e.KeyCode == KeyCode.Down || (Demo.hjkl && e.KeyCode == KeyCode.J)) && currentDialog.options.Count > 1 && currentOption < currentDialog.options.Count - 1)
                    currentOption++;
                else if (e.KeyCode == DialogBrowser.confirmKey && currentDialog.options[currentOption].enabled &&
                         (currentDialog.options[currentOption].linksTo != null ||
                          currentDialog.options[currentOption].eventLink != null
                       || currentDialog.options[currentOption].actionLink != null))
                {
                   // dialogItemForFinish = currentUI.currentDialog.menu[currentUI.currentDialog.currentDialogItem];
                    //dialogItemsForRecall.Add(currentUI.currentDialog.menu[currentUI.currentDialog.currentDialogItem]);
                    currentDialog.options[currentOption].enabled = false;
                    currentDialog.options[currentOption].handleAction(e);
                }
                else if (e.KeyCode == DialogBrowser.backKey)
                {
                    currentOption = (currentDialog.options.Count > 1) ? 1 : 0;
                }
                return;
            }
        }*/

        public static void OnKeyDown_Dialog(InputEventArgs e)
        {
            if (isHidden || Demo.mode != InputMode.Dialog)
            {
                return;
            }
            else
            {
                if ((e.KeyCode == KeyCode.Up || (Demo.hjkl && e.KeyCode == KeyCode.K)) && currentUI.currentDialog.options.Count > 1 && currentUI.currentDialog.currentOption > 0)
                {
                    currentUI.currentDialog.currentOption--;
                    
                }
                else if ((e.KeyCode == KeyCode.Down || (Demo.hjkl && e.KeyCode == KeyCode.J)) && currentUI.currentDialog.options.Count > 1 && currentUI.currentDialog.currentOption < currentUI.currentDialog.options.Count - 1)
                {
                    currentUI.currentDialog.currentOption++;
                }
                else if (e.KeyCode == ScreenBrowser.confirmKey && //currentUI.currentDialog.options[currentUI.currentDialog.currentOption].enabled &&
                        (currentUI.currentDialog.options[currentUI.currentDialog.currentOption].linksTo != null ||
                         currentUI.currentDialog.options[currentUI.currentDialog.currentOption].eventLink != null
                      || currentUI.currentDialog.options[currentUI.currentDialog.currentOption].actionLink != null))
                {
                    dialogItemForFinish = currentUI.currentDialog.options[currentUI.currentDialog.currentOption];
                    dialogItemsForRecall.Add(currentUI.currentDialog.options[currentUI.currentDialog.currentOption]);
                    //currentUI.currentDialog.options[currentUI.currentDialog.currentOption].enabled = false;
                    currentUI.currentDialog.options[currentUI.currentDialog.currentOption].handleAction(e);
                }
                else if (e.KeyCode == ScreenBrowser.backKey && currentUI.currentDialog.previousDialog != null && currentUI.currentDialog != currentUI.initialDialog)
                {
                    //currentUI.currentDialog.options[currentUI.currentDialog.currentOption].enabled = true;
                    NavigateBackward(currentUI.currentDialog);
                    HandleRecall();
                }
                return;
            }
        }
        public static void HandleFinish()
        {
            if (dialogItemForFinish != null)
            {
                dialogItemsForRecall.Clear();
                dialogItemForFinish.handleFinish();
                dialogItemForFinish = null;
            }
        }
        public static void HandleRecall()
        {
            foreach (DialogItem recaller in dialogItemsForRecall)
            {
                recaller.handleRecall();
            }
            dialogItemsForRecall.Clear();
        }
    }
    public class DialogUI
    {
        public Dialog currentDialog;
        // public Dialog previousDialog;
        public Dialog initialDialog;
       // public Dictionary<String, Dialog> allDialogs;
        public List<DialogItem> allDialogItems;
        public FontSurface font;
        public int maxWidth = 30;
        public int maxHeight = 10;
        public bool isHidden;
        public static string mandrillGlyphs = "                                 !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ─━│┃┄┅┆┇┈┉┊┋┌┍┎┏┐┑┒┓└┕┖┗┘┙┚┛├┝┞┟┠┡┢┣┤┥┦┧┨┩┪┫┬┭┮┯┰┱┲┳┴┵┶┷┸┹┺┻┼┽┾┿╀╁╂╃╄╅╆╇╈╉╊╋╌╍╎╏═║╒╓╔╕╖╗╘╙╚╛╜╝╞╟╠╡╢╣╤╥╦╧╨╩╪╫╬╭╮╯╰╱╲╳╴╵╶╷╸╹╺╻╼╽╾╿";
        public static Dictionary<Char, Char> mandrillDict = new Dictionary<Char, Char>();
        static DialogUI()
        {

            int idx = 0;
            foreach (char g in mandrillGlyphs)
            {
                mandrillDict[g] = Char.ConvertFromUtf32(idx)[0];
                idx++;
            }
        }
        public DialogUI(Dialog s, FontSurface fnt)
        {
            initialDialog = s;
            //   previousDialog = s.Clone();
            currentDialog = s;
//            allDialogs = new Dictionary<String, Dialog>() { { s.speaker + s.lines[0], s } };
            allDialogItems = new List<DialogItem>(s.options);
            font = fnt;
        }
        public void Show()
        {
            if (Demo.mode != InputMode.Dialog || currentDialog == null)
                return;
            isHidden = false;
            Dictionary<char, char> mandrillDict = SimpleUI.mandrillDict;
            //string tx;
            /* = mandrillDict['┌'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┐'];*/
            //            for (int i = 1; i < maxWidth - 1; i++)
            //              tx += mandrillDict['─'];
            Display.FillRect(new Rectangle((int)currentDialog.renderX, (int)currentDialog.renderY + 14, 12 + (currentDialog.contentWidth * 6), (currentDialog.contentHeight + 5) * 14), Color.Black);
            int currentLine = 1;
            font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), mandrillDict['┌'] + "".PadRight(currentDialog.contentWidth, mandrillDict['─']) + mandrillDict['┐']);

            currentLine++;
            font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), mandrillDict['│'] + currentDialog.speaker.PadRight(currentDialog.contentWidth, ' ') + mandrillDict['│']);
            /*
            tx = mandrillDict['╞'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['═']);
            tx += mandrillDict['╡'];*/
            currentLine++;
            font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), mandrillDict['╞'] + "".PadRight(currentDialog.contentWidth, mandrillDict['═']) + mandrillDict['╡']);
            foreach (string s in currentDialog.lines)
            {
                currentLine++;
                font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), mandrillDict['│'] + s.PadRight(currentDialog.contentWidth, ' ') + mandrillDict['│']);
            }
            /*
            tx = mandrillDict['├'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┤'];*/
            currentLine++;
            font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), mandrillDict['├'] + "".PadRight(currentDialog.contentWidth, mandrillDict['─']) + mandrillDict['┤']);



            //            if(currentDialog.options.Count > 15)
            List<DialogItem> shownItems = new List<DialogItem>();
            //(Math.Abs((currentDialog.currentOption - 8) * currentDialog.options.Count) + (currentDialog.currentOption - 8)) % currentDialog.options.Count;
            if (currentDialog.options.Count > 15)
            {
                shownItems.AddRange(currentDialog.options.GetRange((currentDialog.currentOption >= 15) ? currentDialog.currentOption - 14 : 0, 15));
                /*
                for (int i = (Math.Abs((currentDialog.currentOption) * currentDialog.options.Count) + (currentDialog.currentOption)) % currentDialog.options.Count
                    , t = 0; t < 15 && t < currentDialog.options.Count; i = (i + 1) % currentDialog.options.Count, t++)
                {
                    shownItems.Add(currentDialog.options[i]);
                }*/
            }
            else
                shownItems = currentDialog.options;
            /*
             
            if (counter + 4 > mb.skillList.Count)
            {
                counter = 0;
                sw.Reset();
            }
             */
            for (int i = 0; i < shownItems.Count; i++)
            {
                currentLine++;
                font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), "" + mandrillDict['│']);
                if (shownItems[i] == currentDialog.options[currentDialog.currentOption])
                    font.Color = Color.Red;
                font.DrawText(currentDialog.renderX + 6, currentDialog.renderY + (14 * currentLine), shownItems[i].text.PadRight(currentDialog.contentWidth, ' '));
                font.Color = Color.White;
                font.DrawText(currentDialog.renderX + 6 + (6 * currentDialog.contentWidth), currentDialog.renderY + (14 * currentLine), "" + mandrillDict['│']);
            }

            currentLine++;
            font.DrawText(currentDialog.renderX, currentDialog.renderY + (14 * currentLine), mandrillDict['└'] + "".PadRight(currentDialog.contentWidth, mandrillDict['─']) + mandrillDict['┘']);
        }/*
        public void ShowOld()
        {
            string tx = mandrillDict['┌'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┐'];
            //            for (int i = 1; i < maxWidth - 1; i++)
            //              tx += mandrillDict['─'];

            font.DrawText(10.0, font.FontHeight * 1, tx);
            font.DrawText(10.0, font.FontHeight * 2, mandrillDict['│'] + currentDialog.speaker.PadRight(maxWidth - 2, ' ') + mandrillDict['│']);

            tx = mandrillDict['├'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┤'];
            font.DrawText(10.0, font.FontHeight * 3, tx);
            for (int i = 0; i < currentDialog.options.Count; i++)
            {
                font.DrawText(10.0, font.FontHeight * (4 + i), "" + mandrillDict['│']);
                if (i == currentDialog.)
                {
                    if (currentDialog.menu[i].enabled)
                        font.Color = Color.Red;
                    else
                        font.Color = Color.Gray;
                    font.DrawText(16.0, font.FontHeight * (4 + i), "> " + currentDialog.menu[i].text.PadRight(maxWidth - 4, ' '));

                }
                else
                {
                    if (currentDialog.menu[i].enabled)
                        font.Color = Color.White;
                    else
                        font.Color = Color.Gray;
                    font.DrawText(16.0, font.FontHeight * (4 + i), currentDialog.menu[i].text.PadRight(maxWidth - 2, ' '));

                }
                font.Color = Color.White;
                font.DrawText(10.0 + 6.0 * (maxWidth - 1), font.FontHeight * (4 + i), "" + mandrillDict['│']);
            }

            tx = mandrillDict['└'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┘'];
            font.DrawText(10.0, font.FontHeight * (4 + currentDialog.options.Count), tx);
        }*/
        public static DialogUI InitUI()
        {
            FontSurface fnt = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
            Dialog initialDialog = new Dialog("The Narrator", new List<String>() {"Welcome to the Unpleasant Dungeon!", "Navigate through menus with the arrow keys.",
                "Confirm a selection with the "+ ScreenBrowser.confirmKey.ToString() + " key.", "Do you understand?"}, new List<DialogItem>()),
                yesDialog = new Dialog("The Narrator", new List<String>() { "Great!", "You can press " + ScreenBrowser.backKey.ToString() + " to go back in menus and dialogs.", "Are you ready to start?" }, new List<DialogItem>()),
                noDialog = new Dialog("The Narrator", new List<String>() { "Then how did you get here?" }, new List<DialogItem>());
            DialogItem yesItem = new DialogItem("Yes", yesDialog, null),
                noItem = new DialogItem("No", noDialog, null),
                endOKItem = new DialogItem("OK!", null, null, DialogBrowser.Hide),
                huhItem = new DialogItem("???", initialDialog, null);
            initialDialog.options.Add(yesItem);
            initialDialog.options.Add(noItem);
            initialDialog.setSize();
            yesDialog.options.Add(endOKItem);
            yesDialog.setSize();
            noDialog.options.Add(huhItem);
            noDialog.setSize();
            yesDialog.previousDialog = initialDialog;
            noDialog.previousDialog = initialDialog;
            //attackChoices.menu.Add(new DialogItem("Scorch", null, Demo.OnKeyDown_SelectSkill));
            DialogUI dui = new DialogUI(initialDialog, fnt);
            dui.allDialogItems.Add(yesItem);
            dui.allDialogItems.Add(noItem);
            return dui;
        }
        public static DialogUI CreateYesNoDialog(string speaker, List<string> startText, LinkedAction yesAction, LinkedAction noAction)
        {
            FontSurface fnt = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
            Dialog initialDialog = new Dialog(speaker, startText, new List<DialogItem>());
            DialogItem yesItem = new DialogItem("Yes", null, null, yesAction),
                noItem = new DialogItem("No", null, null, noAction);
            initialDialog.options.Add(yesItem);
            initialDialog.options.Add(noItem);
            initialDialog.setSize();
            //attackChoices.menu.Add(new DialogItem("Scorch", null, Demo.OnKeyDown_SelectSkill));
            DialogUI dui = new DialogUI(initialDialog, fnt);
            dui.allDialogItems.Add(yesItem);
            dui.allDialogItems.Add(noItem);
            return dui;
        }

        public static void ListenConfirmKey(InputEventArgs e)
        {
            if (ScreenBrowser.backKey != e.KeyCode && KeyCode.Q != e.KeyCode && KeyCode.S != e.KeyCode)
            {
                if (ScreenBrowser.confirmKey != e.KeyCode)
                {
                    DialogBrowser.currentUI.currentDialog.lines[1] = "Confirm is currently " + e.KeyCode.ToString() + ".";
                    DialogBrowser.currentUI.currentDialog.setSize();
                }
                
                ScreenBrowser.confirmKey = e.KeyCode;
                Keyboard.KeyDown -= ListenConfirmKey;
            }
        }
        public static void RegisterConfirmKey()
        {
            Keyboard.KeyDown += ListenConfirmKey;
        }
        public static void ListenBackKey(InputEventArgs e)
        {
            if (ScreenBrowser.confirmKey != e.KeyCode && KeyCode.Q != e.KeyCode && KeyCode.S != e.KeyCode)
            {
                ScreenBrowser.backKey = e.KeyCode;
                DialogBrowser.currentUI.currentDialog.lines[1] = "Back is currently " + ScreenBrowser.backKey.ToString() + ".";
                DialogBrowser.currentUI.currentDialog.setSize();
                Keyboard.KeyDown -= ListenBackKey;
            }
        }
        private static void AssignInitialDialog()
        {
            DialogBrowser.currentUI = InitLoadUI();   
        }
        public static void RegisterBackKey()
        {
            Keyboard.KeyDown += ListenBackKey;
        }
        public static DialogUI InitLoadUI()
        {
            FontSurface fnt = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
            Dialog initialDialog = new Dialog("The Narrator", new List<String>() {"Welcome to the Unpleasant Dungeon!", "Navigate through menus with the arrow keys.",
                "Confirm a selection with the "+ ScreenBrowser.confirmKey.ToString() + " key.", "You can press "+ ScreenBrowser.backKey.ToString() + " to go back in menus and dialogs.", 
                "You can quit this game by pressing Q, which will also give you the option to Save.",
                "Do you want to start a new game, or load a previous game?"}, new List<DialogItem>()),
                finishedLoadDialog = new Dialog("The Narrator", new List<String>() { "Choose a saved game:" }, new List<DialogItem>()),
                remapConfirmDialog = new Dialog("The Narrator", new List<String>() { "Remap the Confirm Key.", "Confirm is currently " + ScreenBrowser.confirmKey.ToString() + ".",
                    "Back is currently " + ScreenBrowser.backKey.ToString() + "." }, new List<DialogItem>()),
                remapBackDialog = new Dialog("The Narrator", new List<String>() { "Remap the Back Key:", "Confirm is currently " + ScreenBrowser.confirmKey.ToString() + "." ,
                    "Back is currently " + ScreenBrowser.backKey.ToString() + "." }, new List<DialogItem>()),
                cannotLoadDialog = new Dialog("The Narrator", new List<String>() { "Play a little first, then you will have a game to load." }, new List<DialogItem>());
            DialogItem newGameItem = new DialogItem("New Game", finishedLoadDialog, null, Demo.Init),
                loadItem = new DialogItem("Load Previous Game", null, null, Demo.LoadStates),
                remapItem = new DialogItem("Use Different Keys", remapConfirmDialog, null, RegisterConfirmKey),
                remapConfirmOKItem = new DialogItem("OK", remapBackDialog, null, RegisterBackKey),
                remapBackOKItem = new DialogItem("OK", initialDialog, null, AssignInitialDialog),
                endOKItem = new DialogItem("OK!", null, null, DialogBrowser.Hide);
            if (!System.IO.File.Exists("save.mobsav"))
                loadItem = new DialogItem("[No Previous Game]", cannotLoadDialog, null);
            initialDialog.options.Add(newGameItem);
            initialDialog.options.Add(loadItem);
            initialDialog.options.Add(remapItem);
            initialDialog.setSize();
            initialDialog.previousDialog = null;
//            finishedLoadDialog.options.Add(endOKItem);
            finishedLoadDialog.setSize();
            finishedLoadDialog.previousDialog = null;
            remapConfirmDialog.options.Add(remapConfirmOKItem);
            remapConfirmDialog.setSize();
            remapConfirmDialog.previousDialog = initialDialog;
            remapBackDialog.options.Add(remapBackOKItem);
            remapBackDialog.setSize();
            remapBackDialog.previousDialog = remapConfirmDialog;
            cannotLoadDialog.options.Add(newGameItem);
            cannotLoadDialog.setSize();
            cannotLoadDialog.previousDialog = null;
            //attackChoices.menu.Add(new DialogItem("Scorch", null, Demo.OnKeyDown_SelectSkill));
            DialogUI dui = new DialogUI(initialDialog, fnt);
            dui.allDialogItems.Add(newGameItem);
            dui.allDialogItems.Add(loadItem);
            return dui;   
        }

        public static DialogUI CreateLoadGameDialog(SortedDictionary<DateTime, Demo.GameState> allSavedStates)
        {
            FontSurface fnt = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
            Dialog chooseDialog = new Dialog("The Narrator", new List<String>() { "Choose a saved game:" }, new List<DialogItem>()),
                finishedLoadDialog = new Dialog("The Narrator", new List<String>() { "Are you ready?" }, new List<DialogItem>()),
                cannotLoadDialog = new Dialog("The Narrator", new List<String>() { "Play a little first, then you will have a game to load." }, new List<DialogItem>());
            DialogItem newGameItem = new DialogItem("New Game", chooseDialog, null, Demo.Init),
                endOKItem = new DialogItem("OK!", null, null, DialogBrowser.Hide);
            if (!System.IO.File.Exists("save.mobsav"))
            {
                chooseDialog.options = new List<DialogItem>(){ new DialogItem("[No Previous Game]", cannotLoadDialog, null)};
            }
            finishedLoadDialog.options.Add(endOKItem);
            finishedLoadDialog.setSize();
            finishedLoadDialog.previousDialog = null;
            //            finishedLoadDialog.options.Add(endOKItem);
            DialogUI dui = new DialogUI(chooseDialog, fnt);
            foreach (DateTime dt in Demo.allSavedStates.Keys)
            {
                chooseDialog.options.Add(new DialogItem("Saved on " + dt.ToShortDateString() + " at " + dt.ToShortTimeString(), null, null, Demo.LoadGame));

            }
            chooseDialog.setSize();
            dui.allDialogItems.AddRange(chooseDialog.options);
            chooseDialog.previousDialog = null;
            cannotLoadDialog.options.Add(newGameItem);
            cannotLoadDialog.setSize();
            cannotLoadDialog.previousDialog = null;
            //attackChoices.menu.Add(new DialogItem("Scorch", null, Demo.OnKeyDown_SelectSkill));
            dui.allDialogItems.Add(newGameItem);
            return dui;   
        }
    }
}
