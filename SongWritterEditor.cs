using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

public class SongWritterEditor : EditorWindow
{
    float tempo;

    Texture[] symbols;
    Texture[] rests;
    bool resting = false;

    Vector2 scrollPos;

    bool legated = false;

    bool editing = false;
    int editingNote = -1;
    int editingRow = -1;

    #region Symbols
    bool semibreve = false;
    bool minim = false;
    bool crotchet = true;
    bool quaver = false;
    bool semiquaver = false;
    bool demisemiquaver = false;

    bool Semibreve
    {
        get
        {
            return semibreve;
        }
        set
        {
            if (value != semibreve)
            {
                semibreve = value;
                ToogleNotes(0);
            }
        }
    }
    bool Minim
    {
        get
        {
            return minim;
        }
        set
        {
            if (value != minim)
            {
                minim = value;
                if (value == true)
                    ToogleNotes(1);
            }
        }
    }
    bool Crotchet
    {
        get
        {
            return crotchet;
        }
        set
        {
            if (value != crotchet)
            {
                crotchet = value;
                ToogleNotes(2);
            }
        }
    }
    bool Quaver
    {
        get
        {
            return quaver;
        }
        set
        {
            if (value != quaver)
            {
                quaver = value; ToogleNotes(3);
            }
        }
    }
    bool Semiquaver
    {
        get
        {
            return semiquaver;
        }
        set
        {
            if (value != semiquaver)
            {
                semiquaver = value; ToogleNotes(4);
            }
        }
    }
    bool Demisemiquaver
    {
        get
        {
            return demisemiquaver;
        }
        set
        {
            if (value != demisemiquaver)
            {
                demisemiquaver = value;
                ToogleNotes(5);
            }
        }
    }

    int currentNote = 2;
    int lastActiveNote = 2;

    bool dotted = false;
    bool tripplet = false;

    Texture semibreveN;
    Texture minimN;
    Texture crotchetN;
    Texture quaverN;
    Texture semiquaverN;
    Texture demisemiquaverN;

    Texture semibreveR;
    Texture minimR;
    Texture crotchetR;
    Texture quaverR;
    Texture semiquaverR;
    Texture demisemiquaverR;

    Texture dotT;
    Texture tripletT;
    #endregion
    #region Notes
    int upNotesCount = 0;
    int leftNotesCount = 0;
    int rightNotesCount = 0;
    int downNotesCount = 0;

    int legatoAmount = 30;

    List<int> total = new List<int>();

    List<int> isLegato = new List<int>();
    List<int> inputTypeList = new List<int>();

    List<int> upNotesSymbol = new List<int>();
    List<int> leftNotesSymbol = new List<int>();
    List<int> rightNotesSymbol = new List<int>();
    List<int> downNotesSymbol = new List<int>();

    #endregion

    [MenuItem("Window/SongWritter")]
    public static void ShowWindow()
    {
        GetWindow<SongWritterEditor>("Song Writter", true, typeof(EditorWindow));
    }

    private void OnEnable()
    {
        Resolution r = Screen.currentResolution;
        minSize = new Vector2(100, r.height / 3);
        maxSize = new Vector2(r.width, r.height);

        semibreveN = Resources.Load("SongWritterImages/1", typeof(Texture)) as Texture;
        minimN = Resources.Load("SongWritterImages/2", typeof(Texture)) as Texture;
        crotchetN = Resources.Load("SongWritterImages/4", typeof(Texture)) as Texture;
        quaverN = Resources.Load("SongWritterImages/8", typeof(Texture)) as Texture;
        semiquaverN = Resources.Load("SongWritterImages/16", typeof(Texture)) as Texture;
        demisemiquaverN = Resources.Load("SongWritterImages/32", typeof(Texture)) as Texture;

        semibreveR = Resources.Load("SongWritterImages/wholeR", typeof(Texture)) as Texture;
        minimR = Resources.Load("SongWritterImages/halfR", typeof(Texture)) as Texture;
        crotchetR = Resources.Load("SongWritterImages/quarterR", typeof(Texture)) as Texture;
        quaverR = Resources.Load("SongWritterImages/eightR", typeof(Texture)) as Texture;
        semiquaverR = Resources.Load("SongWritterImages/16R", typeof(Texture)) as Texture;
        demisemiquaverR = Resources.Load("SongWritterImages/32thR", typeof(Texture)) as Texture;

        dotT = Resources.Load("SongWritterImages/dot", typeof(Texture)) as Texture;
        tripletT = Resources.Load("SongWritterImages/triplet", typeof(Texture)) as Texture;

        symbols = new Texture[] { semibreveN, minimN, crotchetN, quaverN, semiquaverN, demisemiquaverN };
        rests = new Texture[] { semibreveR, minimR, crotchetR, quaverR, semiquaverR, demisemiquaverR };
    }

    public void OnGUI()
    {
        #region Reset
        if (GUILayout.Button("Reset", GUILayout.Width(50), GUILayout.Height(20)))
            reset();
        #endregion
        Color originalColor = GUI.backgroundColor;
        tempo = EditorGUILayout.FloatField("Tempo", tempo, GUILayout.Width(200));

        #region SymbolSelect
        GUILayout.BeginArea(new Rect(5, 50, 1200, 80));
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUI.DrawTexture(new Rect(3, 0, 15, 15), symbols[0]);
                GUI.DrawTexture(new Rect(23, 0, 15, 15), symbols[1]);
                GUI.DrawTexture(new Rect(43, 0, 15, 15), symbols[2]);
                GUI.DrawTexture(new Rect(63, 0, 15, 15), symbols[3]);
                GUI.DrawTexture(new Rect(83, 0, 15, 15), symbols[4]);
                GUI.DrawTexture(new Rect(103, 0, 15, 15), symbols[5]);
                GUI.DrawTexture(new Rect(123, 0, 15, 15), dotT);
                GUI.DrawTexture(new Rect(143, 0, 35, 15), tripletT);
                GUI.Label(new Rect(200, 0, 150, 15), "BLUE is dotted value");
                GUI.Label(new Rect(350, 0, 150, 15), "YELLOW is triplet value");
                GUI.Label(new Rect(700, 0, 150, 15), "GREY NUMBER is TAP");
                GUI.Label(new Rect(700, 15, 150, 15), "RED NUMBER is HOLD");
                GUI.Label(new Rect(700, 30, 175, 15), "ORANGE NUMBER is RELEASE");
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            {
                Semibreve = EditorGUILayout.Toggle(semibreve, GUILayout.Width(15));
                Minim = EditorGUILayout.Toggle(minim, GUILayout.Width(15));
                Crotchet = EditorGUILayout.Toggle(crotchet, GUILayout.Width(15));
                Quaver = EditorGUILayout.Toggle(quaver, GUILayout.Width(17));
                Semiquaver = EditorGUILayout.Toggle(semiquaver, GUILayout.Width(17));
                Demisemiquaver = EditorGUILayout.Toggle(demisemiquaver, GUILayout.Width(17));

                dotted = EditorGUILayout.Toggle(dotted, GUILayout.Width(25));
                tripplet = EditorGUILayout.Toggle(tripplet);
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
        #endregion
        #region Leyend
        GUILayout.BeginArea(new Rect(3, 100, 45, 200), EditorStyles.helpBox); //Notes Names Area
        {
            GUILayout.Space(10);
            GUILayout.Label("Nº");
            GUILayout.Space(17);
            GUILayout.Label("Up");
            GUILayout.Space(17);
            GUILayout.Label("Left");
            GUILayout.Space(17);
            GUILayout.Label("Right");
            GUILayout.Space(17);
            GUILayout.Label("Down");
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndArea();//Notes Names Area
        #endregion
        #region Main Area
        GUILayout.BeginArea(new Rect(50, 100, Screen.width - Screen.width / 25, 200), EditorStyles.helpBox);
        {
            EditorGUILayout.BeginVertical();
            {
                #region Notes Numbers
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width - Screen.width / 23f));
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                int skip = 0;
                for (int i = 0; i < total.Count; i++)
                {
                    legatoAmount = 30;

                    if (inputTypeList[i + skip] == 0)
                        GUI.backgroundColor = originalColor;
                    else if (inputTypeList[i + skip] == 1)
                        GUI.backgroundColor = Color.red;
                    else if (inputTypeList[i + skip] == 2)
                        GUI.backgroundColor = new Color(1f, .5f, .2f);

                    if (isLegato[i + skip] == 1)
                    {
                        legatoAmount = 64;
                        skip++;
                    }
                    else if (isLegato[i + skip] == 2)
                    {
                        legatoAmount = 98;
                        skip += 2;
                    }

                    GUILayout.Box(total[i].ToString(), GUILayout.Width(legatoAmount), GUILayout.Height(30));
                }

                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                #endregion
                #region Notes
                GUILayout.BeginHorizontal();//UP NOTES
                {
                    //GUI.contentColor = Color.green;
                    GUILayout.BeginHorizontal();
                    if (upNotesSymbol.Count > 0)
                    {
                        for (int i = 0; i < upNotesCount; i++)
                        {
                            if (editingNote == i)
                                GUI.backgroundColor = Color.magenta;
                            else
                            {
                                if (upNotesSymbol[i] >= 20 && upNotesSymbol[i] <= 35)
                                    GUI.backgroundColor = Color.cyan;
                                else if (upNotesSymbol[i] >= 40 && upNotesSymbol[i] <= 55)
                                    GUI.backgroundColor = Color.yellow;
                                else
                                    GUI.backgroundColor = originalColor;
                            }

                            if (GUILayout.Button(getTexture(upNotesSymbol[i]), GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                editing = !editing;
                                editingNote = editing == true ? i : -1;
                                editingRow = editing == true ? 0 : -1;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.contentColor = originalColor;
                }
                GUILayout.EndHorizontal();//--UP NOTES
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();//--LEFT NOTES
                {
                    //GUI.contentColor = Color.magenta;
                    if (leftNotesSymbol.Count > 0)
                    {
                        for (int i = 0; i < leftNotesCount; i++)
                        {
                            if (editingNote == i)
                                GUI.backgroundColor = Color.magenta;
                            else
                            {
                                if (leftNotesSymbol[i] >= 20 && leftNotesSymbol[i] <= 35)
                                    GUI.backgroundColor = Color.cyan;
                                else if (leftNotesSymbol[i] >= 40 && leftNotesSymbol[i] <= 55)
                                    GUI.backgroundColor = Color.yellow;
                                else
                                    GUI.backgroundColor = originalColor;
                            }

                            if (GUILayout.Button(getTexture(leftNotesSymbol[i]), GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                editing = !editing;
                                editingNote = editing == true ? i : -1;
                                editingRow = editing == true ? 1 : -1;
                            }
                        }
                    }
                    GUI.contentColor = originalColor;
                }
                GUILayout.EndHorizontal();//--LEFT NOTES
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();//--RIGHT NOTES
                {
                    //GUI.contentColor = Color.red;
                    if (rightNotesSymbol.Count > 0)
                    {
                        for (int i = 0; i < rightNotesCount; i++)
                        {
                            if (editingNote == i)
                                GUI.backgroundColor = Color.magenta;
                            else
                            {
                                if (rightNotesSymbol[i] >= 20 && rightNotesSymbol[i] <= 35)
                                    GUI.backgroundColor = Color.cyan;
                                else if (rightNotesSymbol[i] >= 40 && rightNotesSymbol[i] <= 55)
                                    GUI.backgroundColor = Color.yellow;
                                else
                                    GUI.backgroundColor = originalColor;
                            }

                            if (GUILayout.Button(getTexture(rightNotesSymbol[i]), GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                editing = !editing;
                                editingNote = editing == true ? i : -1;
                                editingRow = editing == true ? 2 : -1;
                            }
                        }
                    }
                    GUI.backgroundColor = originalColor;
                }
                GUILayout.EndHorizontal();//--RIGHT NOTES
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();//--DOWN NOTES
                {
                    //GUI.contentColor = Color.blue;
                    if (downNotesSymbol.Count > 0)
                    {
                        for (int i = 0; i < downNotesCount; i++)
                        {
                            if (editingNote == i)
                                GUI.backgroundColor = Color.magenta;
                            else
                            {
                                if (downNotesSymbol[i] >= 20 && downNotesSymbol[i] <= 35)
                                    GUI.backgroundColor = Color.cyan;
                                else if (downNotesSymbol[i] >= 40 && downNotesSymbol[i] <= 55)
                                    GUI.backgroundColor = Color.yellow;
                                else
                                    GUI.backgroundColor = originalColor;
                            }

                            if (GUILayout.Button(getTexture(downNotesSymbol[i]), GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                editing = !editing;
                                editingNote = editing == true ? i : -1;
                                editingRow = editing == true ? 3 : -1;
                            }
                        }
                    }
                    GUI.backgroundColor = originalColor;
                }
                GUILayout.EndHorizontal();//--DOWN NOTES
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            #endregion
        }
        GUILayout.EndArea();
        EditorGUILayout.EndScrollView();
        #endregion
        #region Save/Load
        GUILayout.BeginArea(new Rect(3, 320, 172, 30), EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load", GUILayout.Height(20), GUILayout.Width(80)))
        {
            load();
        }
        if (GUILayout.Button("Save", GUILayout.Height(20), GUILayout.Width(80)))
        {
            save();
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
        #endregion
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            keyShortCut(e);
        }
        if (e.type == EventType.MouseUp)
        {
            if (e.button == 0)
                addNote(e);
            else if (e.button == 1 && editing)
                removeFromList(false);
            else if (e.button == 2)
                removeFromList(true);
        }

        Repaint();
    }

    void addNote(Event e)
    {
        if (!editing)
        {
            int mod = 0;

            if (dotted)
                mod += 20;
            else if (tripplet)
                mod += 40;

            float posY = e.mousePosition.y;
            if ((posY > 150 && posY < 180))
            {
                addNoteToList(0);
                upNotesSymbol.Add(currentNote + mod);
                upNotesCount++;
            }
            else if ((posY > 190 && posY < 220))
            {
                addNoteToList(1);
                leftNotesSymbol.Add(currentNote + mod);
                leftNotesCount++;
            }
            else if ((posY > 230 && posY < 260))
            {
                addNoteToList(2);
                rightNotesSymbol.Add(currentNote + mod);
                rightNotesCount++;
            }
            else if ((posY > 270 && posY < 300))
            {
                addNoteToList(3);
                downNotesSymbol.Add(currentNote + mod);
                downNotesCount++;
            }
        }
        else
        {
            editing = false;
            editingNote = -1;
            editingRow = -1;
            resting = false;
            legated = false;
        }
    }

    void addNoteToList(int dir)
    {
        if (dir != 0)
        {
            upNotesSymbol.Add(100);
            upNotesCount++;
        }
        if (dir != 1)
        {
            leftNotesSymbol.Add(100);
            leftNotesCount++;
        }
        if (dir != 2)
        {
            rightNotesSymbol.Add(100);
            rightNotesCount++;
        }
        if (dir != 3)
        {
            downNotesSymbol.Add(100);
            downNotesCount++;
        }


        isLegato.Add(0);
        inputTypeList.Add(0);
        total.Add(total.Count + 1);
        scrollPos.x += 40;
    }

    void removeFromList(bool last)
    {
        if (total.Count > 0)
        {
            if (last)
            {
                upNotesSymbol.RemoveAt(upNotesCount - 1);
                leftNotesSymbol.RemoveAt(leftNotesCount - 1);
                rightNotesSymbol.RemoveAt(rightNotesCount - 1);
                downNotesSymbol.RemoveAt(downNotesCount - 1);

                if (isLegato[isLegato.Count - 1] == -1)
                {
                    isLegato[isLegato.Count - 2] = 0;
                    total.Add(total.Count - 1);
                }
                else if (isLegato[isLegato.Count - 1] == -2)
                {
                    isLegato[isLegato.Count - 3] = 1;
                    total.Add(total.Count - 1);
                }

                inputTypeList.RemoveAt(inputTypeList.Count - 1);
                isLegato.RemoveAt(isLegato.Count - 1);
            }
            else
            {
                upNotesSymbol.RemoveAt(editingNote);
                leftNotesSymbol.RemoveAt(editingNote);
                rightNotesSymbol.RemoveAt(editingNote);
                downNotesSymbol.RemoveAt(editingNote);
                isLegato.RemoveAt(editingNote);
                inputTypeList.RemoveAt(editingNote);
            }

            upNotesCount--;
            leftNotesCount--;
            rightNotesCount--;
            downNotesCount--;

            total.RemoveAt(total.Count - 1);
            editingNote = -1;
            editingRow = -1;
            editing = false;
        }

    }

    void keyShortCut(Event e)
    {
        if (e.keyCode == KeyCode.Alpha2)
            currentNote = ++currentNote > 5 ? 0 : currentNote;

        else if (e.keyCode == KeyCode.Alpha1)
            currentNote = --currentNote < 0 ? 5 : currentNote;

        if (e.keyCode == KeyCode.R)
            resting = true;

        if (e.keyCode == KeyCode.Delete && !editing)
            removeFromList(true);

        if (e.keyCode == KeyCode.Insert && editing)
            insertNote();

        if (e.keyCode == KeyCode.A)
            typeOfInput(0);
        if (e.keyCode == KeyCode.S)
            typeOfInput(1);
        if (e.keyCode == KeyCode.D)
            typeOfInput(2);

        if (e.keyCode == KeyCode.L)
            legato();

        if (e.keyCode == KeyCode.KeypadMultiply || e.keyCode == KeyCode.Alpha3)
        {
            dotted = !dotted;
            tripplet = false;
        }
        else if (e.keyCode == KeyCode.KeypadDivide)
        {
            tripplet = !tripplet;
            dotted = false;
        }

        switch (currentNote)
        {
            case 0:
                Semibreve = true;
                break;
            case 1:
                Minim = true;
                break;
            case 2:
                Crotchet = true;
                break;
            case 3:
                Quaver = true;
                break;
            case 4:
                Semiquaver = true;
                break;
            case 5:
                Demisemiquaver = true;
                break;
        }

        if (editing)
            edit();

        Repaint();
    }

    void edit()
    {
        int row = -1;
        int mod = 0;

        if (dotted)
            mod += 20;
        else if (tripplet)
            mod += 40;

        if (!legated)
        {
            switch (editingRow)
            {
                case 0:

                    if (resting)
                        upNotesSymbol[editingNote] = currentNote + 10 + mod;
                    else
                        upNotesSymbol[editingNote] = currentNote + mod;

                    row = 0;
                    break;
                case 1:
                    if (resting)
                        leftNotesSymbol[editingNote] = currentNote + 10 + mod;
                    else
                        leftNotesSymbol[editingNote] = currentNote + mod;

                    row = 1;
                    break;
                case 2:
                    if (resting)
                        rightNotesSymbol[editingNote] = currentNote + 10 + mod;
                    else
                        rightNotesSymbol[editingNote] = currentNote + mod;

                    row = 2;
                    break;
                case 3:
                    if (resting)
                        downNotesSymbol[editingNote] = currentNote + 10 + mod;
                    else
                        downNotesSymbol[editingNote] = currentNote + mod;

                    row = 3;
                    break;
            }

            if (row != 0)
                upNotesSymbol[editingNote] = 100;
            if (row != 1)
                leftNotesSymbol[editingNote] = 100;
            if (row != 2)
                rightNotesSymbol[editingNote] = 100;
            if (row != 3)
                downNotesSymbol[editingNote] = 100;

            legated = false;
        }
    }

    void legato()
    {
        if (!editing)
        {
            if (isLegato.Count > 1)
            {
                if (isLegato[isLegato.Count - 1] == -1)
                {
                    isLegato[isLegato.Count - 1] = 0;
                    isLegato[isLegato.Count - 2] = 0;
                    inputTypeList[inputTypeList.Count - 1] = 0;
                    total.Add(total.Count + 1);
                }
                else if (isLegato[isLegato.Count - 1] == -2)
                {
                    isLegato[isLegato.Count - 1] = 0;
                    isLegato[isLegato.Count - 3] = 1;
                    inputTypeList[inputTypeList.Count - 1] = 0;
                    total.Add(total.Count + 1);
                }
                else
                {
                    if (isLegato[isLegato.Count - 2] == -1)
                    {
                        isLegato[isLegato.Count - 3] = 2;
                        total.RemoveAt(total.Count - 1);
                        isLegato[isLegato.Count - 1] = -2;
                        inputTypeList[inputTypeList.Count - 1] = -1;
                    }
                    else if (isLegato[isLegato.Count - 2] == 0)
                    {
                        isLegato[isLegato.Count - 2] = 1;
                        total.RemoveAt(total.Count - 1);
                        isLegato[isLegato.Count - 1] = -1;
                        inputTypeList[inputTypeList.Count - 1] = -1;
                    }
                }
            }
        }
        else
        {
            if (isLegato.Count > 1)
            {
                if (isLegato[editingNote] == -1 && isLegato[editingNote + 1] != -2)
                {
                    isLegato[editingNote] = 0;
                    isLegato[editingNote - 1] = 0;
                    total.Add(total.Count + 1);
                    inputTypeList[editingNote] = 0;
                }
                else if (isLegato[editingNote] == -2)
                {
                    isLegato[editingNote] = 0;
                    isLegato[editingNote - 2] = 1;
                    total.Add(total.Count + 1);
                    inputTypeList[editingNote] = 0;
                }
                else if (isLegato[editingNote] == 1)
                {
                    isLegato[editingNote] = 0;
                    isLegato[editingNote + 1] = 0;
                    total.Add(total.Count + 1);
                    inputTypeList[editingNote + 1] = 0;
                }
                else if (isLegato[editingNote] == 2)
                {
                    isLegato[editingNote] = 0;
                    isLegato[editingNote + 1] = 0;
                    isLegato[editingNote + 2] = 0;
                    inputTypeList[editingNote + 1] = 0;
                    inputTypeList[editingNote + 2] = 0;
                    total.Add(total.Count + 1);
                    total.Add(total.Count + 1);
                }
                else
                {
                    if (isLegato[editingNote - 1] == -1)
                    {
                        isLegato[editingNote - 2] = 2;
                        total.RemoveAt(total.Count - 1);
                        isLegato[editingNote] = -2;
                        inputTypeList[editingNote] = -1;
                        legated = true;
                    }
                    else if (isLegato[editingNote - 1] == 0)
                    {
                        isLegato[editingNote - 1] = 1;
                        total.RemoveAt(total.Count - 1);
                        isLegato[editingNote] = -1;
                        inputTypeList[editingNote] = -1;
                        legated = true;
                    }
                }
            }
        }
    }

    void typeOfInput(int type)
    {
        if (!editing)
        {
            inputTypeList[inputTypeList.Count - 1] = type;
        }
        else
        {
            inputTypeList[editingNote] = type;
        }
    }

    void insertNote()
    {
        if (isLegato[editingNote] == 0)
        {
            upNotesSymbol.Insert(editingNote, 100);
            leftNotesSymbol.Insert(editingNote, 100);
            rightNotesSymbol.Insert(editingNote, 100);
            downNotesSymbol.Insert(editingNote, 100);

            upNotesCount++;
            leftNotesCount++;
            rightNotesCount++;
            downNotesCount++;

            isLegato.Insert(editingNote, 0);
            inputTypeList.Insert(editingNote, 0);

            total.Add(total.Count + 1);
        }
    }

    void ToogleNotes(int n)
    {
        switch (lastActiveNote)
        {
            case 0:
                semibreve = false;
                break;
            case 1:
                minim = false;
                break;
            case 2:
                crotchet = false;
                break;
            case 3:
                quaver = false;
                break;
            case 4:
                semiquaver = false;
                break;
            case 5:
                demisemiquaver = false;
                break;
        }
        currentNote = n;
        lastActiveNote = n;
    }

    bool load()
    {
        string path = EditorUtility.OpenFilePanel("Open tab", "", "cs");
        if (path.Length == 0)
            return false;

        reset();
        Regex reg = new Regex(@"\(((?<args>\w+\.\w+)(\s*,?\s*))+\)");
        Match match;
        string line;

        StreamReader theReader = new StreamReader(path);

        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();

                if (line != null)
                {
                    match = null;

                    if (line.Contains("BPM"))
                    {
                        tempo = float.Parse(line.Replace("BPM:", ""));
                    }
                    else
                    {
                        match = reg.Match(line);

                        if (match.Success)//if (match.Groups.Count > 0)
                        {
                            Group g = match.Groups["args"];
                            CaptureCollection cc = g.Captures;
                            DoStuff(cc);
                            //for (int j = 0; j < cc.Count; j++)
                            //{
                            //    Capture c = cc[j];
                            //    DoStuff(c.Value.Split('.'), j);
                            //}
                        }
                    }
                }
            }
            while (line != null);

            theReader.Close();
            return true;
        }
    }

    void save()
    {
        string path = EditorUtility.SaveFilePanel("Save tab", "", "Test.cs", "cs");
        if (path.Length == 0)
            return;

        string[] lines = new string[(total.Count * 2) + 1];

        int k = 0;
        int j = 1;
        int lQty = 0;

        for (int i = 0; i < total.Count; i++)
        {
            if (upNotesSymbol[i + lQty] != 100)
                lines[k] = "n = new Note(BeatValue." + getBeatValueToSave(i + lQty, isLegato[i + lQty], 0) + ", TypeOfInput." + getTypeOfInputToSave(inputTypeList[i + lQty], 0);
            else if (leftNotesSymbol[i + lQty] != 100)
                lines[k] = "n = new Note(BeatValue." + getBeatValueToSave(i + lQty, isLegato[i + lQty], 1) + ", TypeOfInput." + getTypeOfInputToSave(inputTypeList[i + lQty], 1);
            else if (rightNotesSymbol[i + lQty] != 100)
                lines[k] = "n = new Note(BeatValue." + getBeatValueToSave(i + lQty, isLegato[i + lQty], 2) + ", TypeOfInput." + getTypeOfInputToSave(inputTypeList[i + lQty], 2);
            else if (downNotesSymbol[i + lQty] != 100)
                lines[k] = "n = new Note(BeatValue." + getBeatValueToSave(i + lQty, isLegato[i + lQty], 3) + ", TypeOfInput." + getTypeOfInputToSave(inputTypeList[i + lQty], 3);// + ", NoteColor.DOWN);";

            lines[j] = "notes.Add(n);";

            if (isLegato[i] == 1)
                lQty += 1;
            else if (isLegato[i] == 2)
                lQty += 2;

            k += 2;
            j += 2;

            //i += lQty;
        }

        lines[lines.Length - 1] = tempo.ToString();
        File.WriteAllLines(path, lines);//(@"C:\Users\Juliten\Desktop\Testo.cs", lines);
    }

    void DoStuff(CaptureCollection cc)
    {
        int note = 100;
        int noteTwo = -1;
        int noteThree = -1;
        int input = 0;
        int color = -1;

        for (int i = 0; i < cc.Count; i++)
        {
            string[] entries = cc[i].Value.Split('.');

            if (entries[0] == "BeatValue")
            {
                if (i == 0)
                {
                    note = getBeatValueToLoad(entries[1]);
                    isLegato.Add(0);
                    total.Add(total.Count + 1);
                }
                else if (i == 1)
                {
                    noteTwo = getBeatValueToLoad(entries[1]);
                    isLegato.Add(-1);
                    isLegato[isLegato.Count - 2] = 1;
                    inputTypeList.Add(-1);
                }
                else if (i == 2)
                {
                    noteThree = getBeatValueToLoad(entries[1]);
                    isLegato.Add(-2);
                    isLegato[isLegato.Count - 3] = 2;
                    inputTypeList.Add(-1);
                }

            }
            else if (entries[0] == "TypeOfInput")
            {
                if (entries[1] == "TAP")
                    input = 0;
                else if (entries[1] == "HOLD")
                    input = 1;
                else if (entries[1] == "RELEASE")
                    input = 2;
                else if (entries[1] == "REST")
                {
                    input = 0;
                    note += 10;
                }

                inputTypeList.Add(input);
            }
            else if (entries[0] == "NoteColor")
            {
                if (entries[1] == "UP")
                    color = 0;
                else if (entries[1] == "LEFT")
                    color = 1;
                else if (entries[1] == "RIGHT")
                    color = 2;
                else if (entries[1] == "DOWN")
                    color = 3;
                else if (entries[1] == "NONE")
                    color = -1;
            }

            if (i == cc.Count - 1)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (j == 1 && noteTwo != -1)
                        note = noteTwo;
                    else if (j == 1 && noteTwo == -1)
                        break;
                    if (j == 2 && noteThree != -1)
                        note = noteThree;
                    else if (j == 2 && noteThree == -1)
                        break;

                    if (color != -1)
                    {
                        switch (color)
                        {
                            case 0:
                                upNotesSymbol.Add(note);
                                //upNotesCount++;
                                break;
                            case 1:
                                leftNotesSymbol.Add(note);
                                //leftNotesCount++;
                                break;
                            case 2:
                                rightNotesSymbol.Add(note);
                                //rightNotesCount++;
                                break;
                            case 3:
                                downNotesSymbol.Add(note);
                                //downNotesCount++;
                                break;
                        }

                        if (color != 0)
                            upNotesSymbol.Add(100);
                        if (color != 1)
                            leftNotesSymbol.Add(100);
                        if (color != 2)
                            rightNotesSymbol.Add(100);
                        if (color != 3)
                            downNotesSymbol.Add(100);
                    }
                    else
                    {
                        upNotesSymbol.Add(note);
                        leftNotesSymbol.Add(note);
                        rightNotesSymbol.Add(note);
                        downNotesSymbol.Add(note);
                    }

                    upNotesCount++;
                    leftNotesCount++;
                    rightNotesCount++;
                    downNotesCount++;
                }
            }
        }
    }

    string getBeatValueToSave(int index, int iterations, int row)
    {
        string b = string.Empty;
        string c = string.Empty;
        for (int i = 0; i <= iterations; i++)
        {
            switch (getRowList(row)[index + i])
            {
                case 0:
                    b = "WholeBeat";
                    break;
                case 10:
                    b = "WholeBeat";
                    inputTypeList[index + i] = 3;
                    break;
                case 20:
                    b = "WholeDottedBeat";
                    break;
                case 30:
                    b = "WholeDottedBeat";
                    inputTypeList[index + i] = 3;
                    break;
                case 40:
                    b = "WholeTripletBeat";
                    break;
                case 50:
                    b = "WholeTripletBeat";
                    inputTypeList[index + i] = 3;
                    break;

                case 1:
                    b = "HalfBeat";
                    break;
                case 11:
                    b = "HalfBeat";
                    inputTypeList[index + i] = 3;
                    break;
                case 21:
                    b = "HalfDottedBeat";
                    break;
                case 31:
                    b = "HalfDottedBeat";
                    inputTypeList[index + i] = 3;
                    break;
                case 41:
                    b = "HalfTripletBeat";
                    break;
                case 51:
                    b = "HalfTripletBeat";
                    inputTypeList[index + i] = 3;
                    break;

                case 2:
                    b = "QuarterBeat";
                    break;
                case 12:
                    inputTypeList[index + i] = 3;
                    b = "QuarterBeat";
                    break;
                case 22:
                    b = "QuarterDottedBeat";
                    break;
                case 32:
                    inputTypeList[index + i] = 3;
                    b = "QuarterDottedBeat";
                    break;
                case 42:
                    b = "QuarterTripletBeat";
                    break;
                case 52:
                    inputTypeList[index + i] = 3;
                    b = "QuarterTripletBeat";
                    break;

                case 3:
                    b = "EighthBeat";
                    break;
                case 13:
                    inputTypeList[index + i] = 3;
                    b = "EighthBeat";
                    break;
                case 23:
                    b = "EighthDottedBeat";
                    break;
                case 33:
                    inputTypeList[index + i] = 3;
                    b = "EighthDottedBeat";
                    break;
                case 43:
                    b = "EighthTripletBeat";
                    break;
                case 53:
                    inputTypeList[index + i] = 3;
                    b = "EighthTripletBeat";
                    break;

                case 4:
                    b = "SixteenthBeat";
                    break;
                case 14:
                    inputTypeList[index + i] = 3;
                    b = "SixteenthBeat";
                    break;
                case 24:
                    b = "SixteenthDottedBeat";
                    break;
                case 34:
                    inputTypeList[index + i] = 3;
                    b = "SixteenthDottedBeat";
                    break;
                case 44:
                    b = "SixteenthTripletBeat";
                    break;
                case 54:
                    inputTypeList[index + i] = 3;
                    b = "SixteenthTripletBeat";
                    break;

                case 5:
                    b = "Thirtysecond";
                    break;
                case 15:
                    inputTypeList[index + i] = 3;
                    b = "Thirtysecond";
                    break;
                case 25:
                    b = "ThirtysecondDotted";
                    break;
                case 35:
                    inputTypeList[index + i] = 3;
                    b = "ThirtysecondDotted";
                    break;
                case 45:
                    b = "ThirtysecondTriplet";
                    break;
                case 55:
                    inputTypeList[index + i] = 3;
                    b = "ThirtysecondTriplet";
                    break;
            }

            if (i < iterations)
                c += b + ", BeatValue.";
            else
                c += b;
        }

        return c;
    }

    int getBeatValueToLoad(string value)
    {
        int b = -1;

        switch (value)
        {
            case "WholeBeat":
                b = 0;
                break;
            case "WholeDottedBeat":
                b = 20;
                break;
            case "WholeTripletBeat":
                b = 40;
                break;

            case "HalfBeat":
                b = 1;
                break;
            case "HalfDottedBeat":
                b = 21;
                break;
            case "HalfTripletBeat":
                b = 41;
                break;

            case "QuarterBeat":
                b = 2;
                break;
            case "QuarterDottedBeat":
                b = 22;
                break;
            case "QuarterTripletBeat":
                b = 42;
                break;

            case "EighthBeat":
                b = 3;
                break;
            case "EighthDottedBeat":
                b = 23;
                break;
            case "EighthTripletBeat":
                b = 43;
                break;

            case "SixteenthBeat":
                b = 4;
                break;
            case "SixteenthDottedBeat":
                b = 24;
                break;
            case "SixteenthTripletBeat":
                b = 44;
                break;

            case "Thirtysecond":
                b = 5;
                break;
            case "ThirtysecondDotted":
                b = 25;
                break;
            case "ThirtysecondTriplet":
                b = 45;
                break;
        }

        return b;
    }

    string getTypeOfInputToSave(int value, int row)
    {
        string t = "";

        if (value == 3)
        {
            t = "REST, NoteColor.NONE);";
        }
        else
        {
            switch (value)
            {
                case 0:
                    t = "TAP";
                    break;
                case 1:
                    t = "HOLD";
                    break;
                case 2:
                    t = "RELEASE";
                    break;
            }

            switch (row)
            {
                case 0:
                    t += ", NoteColor.UP);";
                    break;
                case 1:
                    t += ", NoteColor.LEFT);";
                    break;
                case 2:
                    t += ", NoteColor.RIGHT);";
                    break;
                case 3:
                    t += ", NoteColor.DOWN);";
                    break;
            }
        }


        return t;
    }

    List<int> getRowList(int row)
    {
        List<int> t = new List<int>();

        if (row == 0)
            t = upNotesSymbol;
        else if (row == 1)
            t = leftNotesSymbol;
        else if (row == 2)
            t = rightNotesSymbol;
        else
            t = downNotesSymbol;

        return t;
    }

    Texture getTexture(int n)
    {
        Texture b = null;
        switch (n)
        {
            case 0:
            case 20:
            case 40:
                b = symbols[0];
                break;
            case 1:
            case 21:
            case 41:
                b = symbols[1];
                break;
            case 2:
            case 22:
            case 42:
                b = symbols[2];
                break;
            case 3:
            case 23:
            case 43:
                b = symbols[3];
                break;
            case 4:
            case 24:
            case 44:
                b = symbols[4];
                break;
            case 5:
            case 25:
            case 45:
                b = symbols[5];
                break;
            case 10:
            case 30:
            case 50:
                b = rests[0];
                break;
            case 11:
            case 31:
            case 51:
                b = rests[1];
                break;
            case 12:
            case 32:
            case 52:
                b = rests[2];
                break;
            case 13:
            case 33:
            case 53:
                b = rests[3];
                break;
            case 14:
            case 34:
            case 54:
                b = rests[4];
                break;
            case 15:
            case 35:
            case 55:
                b = rests[5];
                break;
            case 100:
                b = dotT;
                break;
        }

        return b;
    }

    void reset()
    {
        upNotesSymbol.Clear();
        leftNotesSymbol.Clear();
        rightNotesSymbol.Clear();
        downNotesSymbol.Clear();

        upNotesCount = 0;
        leftNotesCount = 0;
        rightNotesCount = 0;
        downNotesCount = 0;

        isLegato.Clear();
        inputTypeList.Clear();
        total.Clear();
        editingNote = -1;
        editing = false;
    }
}