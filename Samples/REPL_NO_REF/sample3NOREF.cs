// ════════════════════════════════════════════════════
//   CALCULATOR  ·  dark / light theme  ·  UITK Row layout
// ════════════════════════════════════════════════════

// ── State ─────────────────────────────────────────────
string _disp     = "0";
double _acc      = 0;
string _op       = "";
bool   _new      = true;
bool   _dark     = true;
bool   _ext      = false;
bool   _extDirty = false;

// ── Dark palette ──────────────────────────────────────
var D_BG   = new UnityEngine.Color(0.09f, 0.09f, 0.11f, 1f);
var D_DISP = new UnityEngine.Color(0.13f, 0.13f, 0.16f, 1f);
var D_DIG  = new UnityEngine.Color(0.20f, 0.20f, 0.24f, 1f);
var D_OP   = new UnityEngine.Color(0.82f, 0.46f, 0.00f, 1f);
var D_EQ   = new UnityEngine.Color(0.14f, 0.54f, 0.24f, 1f);
var D_CLR  = new UnityEngine.Color(0.48f, 0.13f, 0.13f, 1f);
var D_SCI  = new UnityEngine.Color(0.14f, 0.22f, 0.36f, 1f);
var D_TXT  = new UnityEngine.Color(0.95f, 0.95f, 0.98f, 1f);
var D_DIM  = new UnityEngine.Color(0.48f, 0.48f, 0.54f, 1f);
var D_BORD = new UnityEngine.Color(0.28f, 0.28f, 0.32f, 1f);

// ── Light palette ─────────────────────────────────────
var L_BG   = new UnityEngine.Color(0.93f, 0.93f, 0.95f, 1f);
var L_DISP = new UnityEngine.Color(1.00f, 1.00f, 1.00f, 1f);
var L_DIG  = new UnityEngine.Color(0.86f, 0.86f, 0.89f, 1f);
var L_OP   = new UnityEngine.Color(1.00f, 0.62f, 0.10f, 1f);
var L_EQ   = new UnityEngine.Color(0.18f, 0.74f, 0.36f, 1f);
var L_CLR  = new UnityEngine.Color(0.95f, 0.32f, 0.32f, 1f);
var L_SCI  = new UnityEngine.Color(0.66f, 0.80f, 0.98f, 1f);
var L_TXT  = new UnityEngine.Color(0.08f, 0.08f, 0.10f, 1f);
var L_DIM  = new UnityEngine.Color(0.44f, 0.44f, 0.50f, 1f);
var L_BORD = new UnityEngine.Color(0.72f, 0.72f, 0.76f, 1f);

// ── Helper: lighter/darker for hover and press ────────
Func<UnityEngine.Color, UnityEngine.Color> lighter = c => new UnityEngine.Color(
    UnityEngine.Mathf.Min(c.r + 0.12f, 1f),
    UnityEngine.Mathf.Min(c.g + 0.12f, 1f),
    UnityEngine.Mathf.Min(c.b + 0.12f, 1f), c.a);
Func<UnityEngine.Color, UnityEngine.Color> darker = c => new UnityEngine.Color(
    c.r * 0.70f, c.g * 0.70f, c.b * 0.70f, c.a);

// ── Panel — double width, less tall ───────────────────
// ContentW ≈ 500 - 18 - 6 = 476  →  4 × 115 + 3 × 4 = 472 ✓
var p = CMS2026UITKFramework.FrameworkAPI.CreatePanel("Calculator", 60, 40, 500, 430, sortOrder: 9999);
p.SetScrollbarVisible(false);
p.SetDragWhenScrollable(true);

var pve = CMS2026UITKFramework.UIRuntime.WrapVE(p.GetPanelRawPtr());
var pst = CMS2026UITKFramework.UIRuntime.GetStyle(pve);
CMS2026UITKFramework.S.BorderRadius(pst, 18f);
CMS2026UITKFramework.S.BorderWidth(pst, 1.5f);
CMS2026UITKFramework.S.BgColor(pst, D_BG);
CMS2026UITKFramework.S.BorderColor(pst, D_BORD);

// ── Display ───────────────────────────────────────────
var lblExpr = p.AddLabel("", D_DIM, height: 20f);
lblExpr.SetFontSize(11);

var lblMain = p.AddLabel("0", D_TXT, height: 58f);
lblMain.SetFontSize(32);

var dve = CMS2026UITKFramework.UIRuntime.WrapVE(lblMain.GetRawPtr());
var dst = CMS2026UITKFramework.UIRuntime.GetStyle(dve);
CMS2026UITKFramework.S.BgColor(dst, D_DISP);
CMS2026UITKFramework.S.BorderRadius(dst, 10f);
CMS2026UITKFramework.S.Padding(dst, 8f);
CMS2026UITKFramework.S.TextAlign(dst, UnityEngine.TextAnchor.MiddleRight);

p.AddSpace(8f);

// ── Logic ─────────────────────────────────────────────
Action refresh = () => lblMain.SetText(_disp);

Action<string> digit = d => {
    if (_new) { _disp = d == "." ? "0." : d; _new = false; }
    else {
        if (d == "." && _disp.Contains(".")) return;
        if (_disp.Length >= 12) return;
        _disp = (_disp == "0" && d != ".") ? d : _disp + d;
    }
    refresh();
};

Func<double> cur = () => {
    double v = 0;
    double.TryParse(_disp,
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out v);
    return v;
};

Func<double, string> fmt = v => {
    if (double.IsNaN(v))      return "Error";
    if (double.IsInfinity(v)) return "Inf";
    if (v == Math.Floor(v) && Math.Abs(v) < 1e15) return ((long)v).ToString();
    return v.ToString("G9", System.Globalization.CultureInfo.InvariantCulture);
};

Action<string> pressOp = o => {
    double c = cur();
    if (_op != "" && !_new) {
        switch (_op) {
            case "+": _acc += c; break;
            case "-": _acc -= c; break;
            case "x": _acc *= c; break;
            case "/": _acc = c != 0 ? _acc / c : double.NaN; break;
        }
        _disp = fmt(_acc);
    } else { _acc = c; }
    _op = o; _new = true;
    lblExpr.SetText(fmt(_acc) + "  " + o);
    refresh();
};

Action pressEq = () => {
    if (_op == "") return;
    double c = cur(), res = _acc;
    switch (_op) {
        case "+": res = _acc + c; break;
        case "-": res = _acc - c; break;
        case "x": res = _acc * c; break;
        case "/": res = c != 0 ? _acc / c : double.NaN; break;
    }
    lblExpr.SetText(fmt(_acc) + " " + _op + " " + fmt(c) + " =");
    _disp = fmt(res); _acc = res; _op = ""; _new = true;
    refresh();
};

Action clear = () => {
    _disp = "0"; _acc = 0; _op = ""; _new = true;
    lblExpr.SetText(""); refresh();
};

Action<Func<double,double>> sci = f => {
    _disp = fmt(f(cur())); _new = true; refresh();
};

// ── Button dimensions ─────────────────────────────────
const float BW  = 115f;  // 4-column button width
const float BH  = 48f;   // row height
const float GAP = 4f;
const float BW3 = 156f;  // 3-column button width (scientific)

// ── Row 1: AC  DEL  +/-  % ───────────────────────────
var r1   = p.AddRow(BH, GAP);
var bAC  = r1.AddButton("AC",  BW, () => clear(), D_CLR);
var bDel = r1.AddButton("DEL", BW, () => {
    if (!_new && _disp.Length > 1) { _disp = _disp.Substring(0, _disp.Length-1); refresh(); }
    else { _disp = "0"; _new = true; refresh(); }
}, D_CLR);
var bPM  = r1.AddButton("+/-", BW, () => {
    if (_disp == "0") return;
    _disp = _disp.StartsWith("-") ? _disp.Substring(1) : "-" + _disp;
    refresh();
}, D_SCI);
var bPct = r1.AddButton("%", BW, () => { _disp = fmt(cur() / 100.0); refresh(); }, D_SCI);

p.AddSpace(GAP);

// ── Row 2: 7  8  9  ÷ ────────────────────────────────
var r2   = p.AddRow(BH, GAP);
var b7   = r2.AddButton("7", BW, () => digit("7"), D_DIG);
var b8   = r2.AddButton("8", BW, () => digit("8"), D_DIG);
var b9   = r2.AddButton("9", BW, () => digit("9"), D_DIG);
var bDiv = r2.AddButton("÷", BW, () => pressOp("/"), D_OP);

p.AddSpace(GAP);

// ── Row 3: 4  5  6  × ────────────────────────────────
var r3   = p.AddRow(BH, GAP);
var b4   = r3.AddButton("4", BW, () => digit("4"), D_DIG);
var b5   = r3.AddButton("5", BW, () => digit("5"), D_DIG);
var b6   = r3.AddButton("6", BW, () => digit("6"), D_DIG);
var bMul = r3.AddButton("×", BW, () => pressOp("x"), D_OP);

p.AddSpace(GAP);

// ── Row 4: 1  2  3  − ────────────────────────────────
var r4   = p.AddRow(BH, GAP);
var b1   = r4.AddButton("1", BW, () => digit("1"), D_DIG);
var b2   = r4.AddButton("2", BW, () => digit("2"), D_DIG);
var b3   = r4.AddButton("3", BW, () => digit("3"), D_DIG);
var bSub = r4.AddButton("−", BW, () => pressOp("-"), D_OP);

p.AddSpace(GAP);

// ── Row 5: 0  .  =  + ────────────────────────────────
var r5   = p.AddRow(BH, GAP);
var b0   = r5.AddButton("0", BW, () => digit("0"), D_DIG);
var bDt  = r5.AddButton(".", BW, () => digit("."),  D_DIG);
var bEq  = r5.AddButton("=", BW, () => pressEq(),   D_EQ);
var bAdd = r5.AddButton("+", BW, () => pressOp("+"), D_OP);

p.AddSpace(10f);
p.AddSeparator();

// ── Extended toggle ───────────────────────────────────
p.AddToggle("Extended", false, v => { _ext = v; _extDirty = true; });
p.AddSpace(4f);

// ── Scientific rows: 3 columns ────────────────────────
var rs1  = p.AddRow(38f, GAP);
var bSin = rs1.AddButton("sin", BW3, () => sci(x => Math.Sin(x * Math.PI / 180.0)), D_SCI);
var bCos = rs1.AddButton("cos", BW3, () => sci(x => Math.Cos(x * Math.PI / 180.0)), D_SCI);
var bTan = rs1.AddButton("tan", BW3, () => sci(x => Math.Tan(x * Math.PI / 180.0)), D_SCI);

p.AddSpace(GAP);

var rs2  = p.AddRow(38f, GAP);
var bSqr = rs2.AddButton("√x",  BW3, () => sci(x => Math.Sqrt(x)),     D_SCI);
var bSq2 = rs2.AddButton("x²",  BW3, () => sci(x => x * x),            D_SCI);
var bLog = rs2.AddButton("log", BW3, () => sci(x => Math.Log10(x)),     D_SCI);

p.AddSpace(GAP);

var rs3  = p.AddRow(38f, GAP);
var bLn  = rs3.AddButton("ln", BW3, () => sci(x => Math.Log(x)),        D_SCI);
var bPi  = rs3.AddButton("π",  BW3, () => { _disp = fmt(Math.PI); _new = false; refresh(); }, D_SCI);
var bEul = rs3.AddButton("e",  BW3, () => { _disp = fmt(Math.E);  _new = false; refresh(); }, D_SCI);

bSin.SetVisible(false); bCos.SetVisible(false); bTan.SetVisible(false);
bSqr.SetVisible(false); bSq2.SetVisible(false); bLog.SetVisible(false);
bLn.SetVisible(false);  bPi.SetVisible(false);  bEul.SetVisible(false);

p.AddSpace(10f);
p.AddSeparator();

// ── Theme toggle ──────────────────────────────────────
p.AddToggle("☀  Light theme", false, v => {
    _dark = !v;
    bool d = _dark;

    // Panel and display background
    CMS2026UITKFramework.S.BgColor(pst,     d ? D_BG   : L_BG);
    CMS2026UITKFramework.S.BorderColor(pst, d ? D_BORD  : L_BORD);
    CMS2026UITKFramework.S.BgColor(dst,     d ? D_DISP  : L_DISP);
    lblMain.SetColor(d ? D_TXT : L_TXT);
    lblExpr.SetColor(d ? D_DIM : L_DIM);

    // Helper: recolor button AND rewire hover with correct palette colors
    Action<CMS2026UITKFramework.UIButtonHandle, UnityEngine.Color> recolor =
        (btn, bg) => {
            btn.SetBgColor(bg);
            btn.SetTextColor(d ? D_TXT : L_TXT);
            // Rewire hover so it uses the new base color
            p.WireHover(btn.GetRawPtr(), bg, lighter(bg), darker(bg));
        };

    recolor(bAC,  d ? D_CLR : L_CLR);
    recolor(bDel, d ? D_CLR : L_CLR);
    recolor(bPM,  d ? D_SCI : L_SCI);
    recolor(bPct, d ? D_SCI : L_SCI);

    recolor(bDiv, d ? D_OP : L_OP);
    recolor(bMul, d ? D_OP : L_OP);
    recolor(bSub, d ? D_OP : L_OP);
    recolor(bAdd, d ? D_OP : L_OP);

    foreach (var b in new[]{b0,b1,b2,b3,b4,b5,b6,b7,b8,b9,bDt})
        recolor(b, d ? D_DIG : L_DIG);

    recolor(bEq, d ? D_EQ : L_EQ);

    foreach (var b in new[]{bSin,bCos,bTan,bSqr,bSq2,bLog,bLn,bPi,bEul})
        recolor(b, d ? D_SCI : L_SCI);
});

// ── Update callback — extended section visibility ─────
p.SetUpdateCallback(dt => {
    if (!_extDirty) return;
    _extDirty = false;
    bSin.SetVisible(_ext); bCos.SetVisible(_ext); bTan.SetVisible(_ext);
    bSqr.SetVisible(_ext); bSq2.SetVisible(_ext); bLog.SetVisible(_ext);
    bLn.SetVisible(_ext);  bPi.SetVisible(_ext);  bEul.SetVisible(_ext);
});

Print("Calculator ready!");