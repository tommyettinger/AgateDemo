using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;
using AgateLib.InputLib;
using AgateDemo;

namespace AgateDemo
{
    public delegate void LinkedEvent(InputEventArgs e);
    public delegate void LinkedAction();
    public static class ScreenBrowser
    {
        //        public static Screen endScreen;
        public static SimpleUI currentUI;
        public static KeyCode confirmKey = KeyCode.Z, backKey = KeyCode.X;
        public static MenuItem menuItemForFinish = null;
        public static List<MenuItem> menuItemsForRecall = new List<MenuItem>();
        public static bool isHidden = false;
        //        public static LinkedAction associatedEvent;
        public static void Init()
        {
            //            associatedEvent = menuEventHandler;
            //endScreen = new Screen("__END_SCREEN__", new List<MenuItem>());
            currentUI = SimpleUI.InitUI();

        }
        public static void Navigate(Screen s)
        {
            // currentUI.initialScreen = currentUI.currentScreen.Clone();
            //            s.previousScreen = currentUI.currentScreen.Clone();
            currentUI.currentScreen = s;
        }
        public static void NavigateBackward(Screen s)
        {
            // currentUI.initialScreen = currentUI.currentScreen.Clone();
            // currentUI.previousScreen = currentUI.currentScreen.Clone();
            currentUI.currentScreen = s.previousScreen;
        }
        public static void Hide()
        {
            isHidden = true;
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
            currentUI.currentScreen = currentUI.initialScreen.Clone();
            foreach (MenuItem mi in currentUI.allMenuItems)
            {
                mi.enabled = true;
            }
            //            return s;
        }
        public static void OnKeyDown_Menu(InputEventArgs e)
        {
            if (isHidden)
            {
                return;
            }
            else
            {
                if (e.KeyCode == KeyCode.Up && currentUI.currentScreen.menu.Count > 1 && currentUI.currentScreen.currentMenuItem > 0)
                    currentUI.currentScreen.currentMenuItem--;
                else if (e.KeyCode == KeyCode.Down && currentUI.currentScreen.menu.Count > 1 && currentUI.currentScreen.currentMenuItem < currentUI.currentScreen.menu.Count - 1)
                    currentUI.currentScreen.currentMenuItem++;
                else if (e.KeyCode == confirmKey && currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].enabled &&
                         (currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].linksTo != null || currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].eventLink != null
                       || currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].actionLink != null))
                {
                    menuItemForFinish = currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem];
                    menuItemsForRecall.Add(currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem]);
                    currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].enabled = false;
                    currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].handleAction(e);
                }
                else if (e.KeyCode == backKey && currentUI.currentScreen.previousScreen != null && currentUI.currentScreen != currentUI.initialScreen)
                {
                    currentUI.currentScreen.menu[currentUI.currentScreen.currentMenuItem].enabled = true;
                    NavigateBackward(currentUI.currentScreen);
                    HandleRecall();
                }
            }
        }
        public static void HandleFinish()
        {
            if (menuItemForFinish != null)
            {
                menuItemForFinish.handleFinish();
                menuItemForFinish = null;
            }
        }
        public static void HandleRecall()
        {
            foreach (MenuItem recaller in menuItemsForRecall)
            {
                recaller.handleRecall();
            }
            menuItemsForRecall.Clear();
        }
    }
    public class MenuItem
    {
        public bool enabled;
        public string text;
        public Screen linksTo;
        public InputEventHandler eventLink;
        public LinkedAction actionLink;
        public MenuItem(string txt, Screen screenLink, LinkedEvent evLink)
        {
            enabled = true;
            text = txt;
            linksTo = screenLink;
            if (evLink != null)
            {
                eventLink = new InputEventHandler(evLink);
            }
            else
            {
                eventLink = null;
            }
        }
        public MenuItem(string txt, Screen screenLink, LinkedEvent evLink, LinkedAction act)
        {
            enabled = true;
            text = txt;
            linksTo = screenLink;
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
        public MenuItem(string txt, Screen screenLink, LinkedEvent evLink, bool isEnabled)
        {
            enabled = isEnabled;
            text = txt;
            linksTo = screenLink;
            if (evLink != null)
            {
                eventLink = new InputEventHandler(evLink);
            }
            else
            {
                eventLink = null;
            }
            actionLink = null;
        }
        public void handleAction(InputEventArgs e)
        {
            if (linksTo != null)
                ScreenBrowser.Navigate(linksTo);
            else if (eventLink != null)
            {
                ScreenBrowser.Hide();
                Keyboard.KeyDown -= new InputEventHandler(ScreenBrowser.OnKeyDown_Menu);
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
                ScreenBrowser.Navigate(ScreenBrowser.currentUI.initialScreen);
                enabled = false;
                Keyboard.KeyDown -= eventLink;
            }
            else if (actionLink != null)
            {
                ScreenBrowser.Navigate(ScreenBrowser.currentUI.initialScreen);
                enabled = true;
                ScreenBrowser.Hide();
            }
        }
        public void handleRecall()
        {
            if (linksTo == null && eventLink != null)
            {
                ScreenBrowser.Navigate(ScreenBrowser.currentUI.initialScreen);
                enabled = true;
                Keyboard.KeyDown -= eventLink;
            }
            else if (linksTo != null)
            {
                enabled = true;
            }
        }
    }
    public class Screen
    {
        public string title;
        public List<MenuItem> menu;
        public int currentMenuItem = 0;
        public Screen previousScreen = null;
        public Screen(string ttl, List<MenuItem> menus)
        {
            title = ttl;
            menu = menus;
            if (menu.Count > 0)
                currentMenuItem = 0;
        }
        public Screen Clone()
        {
            return new Screen(title, menu.ToList());
        }
    }
    public class SimpleUI
    {
        public Screen currentScreen;
        // public Screen previousScreen;
        public Screen initialScreen;
        public Dictionary<String, Screen> allScreens;
        public List<MenuItem> allMenuItems;
        public FontSurface font;
        public int maxWidth = 30;
        public int maxHeight = 10;
        public static string mandrillGlyphs = "                                 !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ─━│┃┄┅┆┇┈┉┊┋┌┍┎┏┐┑┒┓└┕┖┗┘┙┚┛├┝┞┟┠┡┢┣┤┥┦┧┨┩┪┫┬┭┮┯┰┱┲┳┴┵┶┷┸┹┺┻┼┽┾┿╀╁╂╃╄╅╆╇╈╉╊╋╌╍╎╏═║╒╓╔╕╖╗╘╙╚╛╜╝╞╟╠╡╢╣╤╥╦╧╨╩╪╫╬╭╮╯╰╱╲╳╴╵╶╷╸╹╺╻╼╽╾╿";
        public static Dictionary<Char, Char> mandrillDict = new Dictionary<Char, Char>();
        static SimpleUI()
        {

            int idx = 0;
            foreach (char g in mandrillGlyphs)
            {
                mandrillDict[g] = Char.ConvertFromUtf32(idx)[0];
                idx++;
            }
        }
        public SimpleUI(Screen s, FontSurface fnt)
        {
            initialScreen = s.Clone();
            //   previousScreen = s.Clone();
            currentScreen = s;
            allScreens = new Dictionary<String, Screen>() { { s.title, s } };
            allMenuItems = new List<MenuItem>(s.menu);
            font = fnt;
        }
        public void Show()
        {
            string tx = mandrillDict['┌'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┐'];
            //            for (int i = 1; i < maxWidth - 1; i++)
            //              tx += mandrillDict['─'];

            font.DrawText(10.0, font.FontHeight * 1, tx);
            font.DrawText(10.0, font.FontHeight * 2, mandrillDict['│'] + currentScreen.title.PadRight(maxWidth - 2, ' ') + mandrillDict['│']);

            tx = mandrillDict['├'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┤'];
            font.DrawText(10.0, font.FontHeight * 3, tx);
            for (int i = 0; i < currentScreen.menu.Count; i++)
            {
                font.DrawText(10.0, font.FontHeight * (4 + i), "" + mandrillDict['│']);
                if (i == currentScreen.currentMenuItem)
                {
                    if (currentScreen.menu[i].enabled)
                        font.Color = Color.Red;
                    else
                        font.Color = Color.Gray;
                    font.DrawText(16.0, font.FontHeight * (4 + i), "> " + currentScreen.menu[i].text.PadRight(maxWidth - 4, ' '));

                }
                else
                {
                    if (currentScreen.menu[i].enabled)
                        font.Color = Color.White;
                    else
                        font.Color = Color.Gray;
                    font.DrawText(16.0, font.FontHeight * (4 + i), currentScreen.menu[i].text.PadRight(maxWidth - 2, ' '));

                }
                font.Color = Color.White;
                font.DrawText(10.0 + 6.0 * (maxWidth - 1), font.FontHeight * (4 + i), "" + mandrillDict['│']);
            }

            tx = mandrillDict['└'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┘'];
            font.DrawText(10.0, font.FontHeight * (4 + currentScreen.menu.Count), tx);
        }
        public static SimpleUI InitUI()
        {
            FontSurface fnt = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
            Screen initialActionChoices = new Screen("Act!", new List<MenuItem>()),
                attackChoices = new Screen("Attack!", new List<MenuItem>());
            MenuItem moveItem = new MenuItem("Move", null, Demo.OnKeyDown_SelectMove, Demo.HighlightMove),
                attackItem = new MenuItem("Attack", attackChoices, null),
                waitItem = new MenuItem("Wait", null, null, Demo.waitAction),
                lookItem = new MenuItem("Look", null, Demo.OnKeyDown_LookAround);
            initialActionChoices.menu.Add(moveItem);
            initialActionChoices.menu.Add(attackItem);
            initialActionChoices.menu.Add(waitItem);
            initialActionChoices.menu.Add(lookItem);
            attackChoices.previousScreen = initialActionChoices;
            //attackChoices.menu.Add(new MenuItem("Scorch", null, Demo.OnKeyDown_SelectSkill));
            SimpleUI sui = new SimpleUI(initialActionChoices, fnt);
            sui.allScreens.Add("Act", initialActionChoices);
            sui.allScreens.Add("Attack", attackChoices);
            sui.allMenuItems.Add(moveItem);
            sui.allMenuItems.Add(attackItem);
            sui.allMenuItems.Add(waitItem);
            sui.allMenuItems.Add(lookItem);
            return sui;
        }
        public void addSkills(Demo.Mob mb)
        {
            foreach (Skill sk in mb.skillList)
            {
                MenuItem skmi = new MenuItem(sk.name, null, Demo.OnKeyDown_SelectSkill, Demo.HighlightSkill);
                allScreens["Attack"].menu.Add(skmi);
                allMenuItems.Add(skmi);
            }
        }

    }
    public static class SkillView
    {
        //        public Skill sk;
        public static string RangeToString(Skill sk)
        {
            string current = "Range: ";
            if (sk.maxSkillDistance == 0)
                current += "Personal";
            else if (sk.minSkillDistance == sk.maxSkillDistance)
                current += sk.minSkillDistance;
            else
                current += "" + sk.minSkillDistance + "-" + sk.maxSkillDistance;
            return current;
        }
    }
    public class UnitInfo
    {
        public static int maxWidth = 30;
        public static int maxHeight = 15;
        public static int renderX = 760;
        public static int renderY = 420;
        public static void ShowMobInfo(Demo.Mob mb)
        {
            Dictionary<char, char> mandrillDict = SimpleUI.mandrillDict;
            string tx = mandrillDict['┌'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┐'];
            //            for (int i = 1; i < maxWidth - 1; i++)
            //              tx += mandrillDict['─'];

            int currentLine = 1;
            mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), tx);
            currentLine++;
            mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), mandrillDict['│'] + (mb.name + ((mb.friendly == true) ? " (Ally)" : "")).PadRight(maxWidth - 2, ' ') + mandrillDict['│']);

            tx = mandrillDict['╞'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['═']);
            tx += mandrillDict['╡'];
            currentLine++;
            mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), tx);

            currentLine++;
            mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), mandrillDict['│'] + (mb.health + "/" + mb.maxHP + " HP").PadRight(maxWidth - 2, ' ') + mandrillDict['│']);

            tx = mandrillDict['├'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┤'];
            currentLine++;
            mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), tx);
            
            for (int i = 0; i < mb.skillList.Count; i++)
            {
                currentLine++;
                mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), "" + mandrillDict['│']);
                mb.ui.font.DrawText(renderX + 6.0, renderY + (mb.ui.font.FontHeight * (currentLine)), mb.skillList[i].name.PadRight(maxWidth - 2, ' '));
                mb.ui.font.DrawText(renderX + 6.0 * (maxWidth - 1), renderY + (mb.ui.font.FontHeight * currentLine), "" + mandrillDict['│']);

                currentLine++;
                mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), "" + mandrillDict['│']);
                mb.ui.font.DrawText(renderX + 6.0, renderY + (mb.ui.font.FontHeight * (currentLine)), ("Damage: " + mb.skillList[i].damage + "   " + SkillView.RangeToString(mb.skillList[i])).PadRight(maxWidth - 2, ' '));
                mb.ui.font.DrawText(renderX + 6.0 * (maxWidth - 1), renderY + (mb.ui.font.FontHeight * currentLine), "" + mandrillDict['│']);
            }
            tx = mandrillDict['└'].ToString();
            tx = tx.PadRight(maxWidth - 1, mandrillDict['─']);
            tx += mandrillDict['┘'];
            currentLine++; 
            mb.ui.font.DrawText(renderX, renderY + (mb.ui.font.FontHeight * currentLine), tx);
        }
    }
}
