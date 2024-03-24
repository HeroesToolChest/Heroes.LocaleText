namespace Heroes.LocaleText.Tests;

[TestClass]
public class DescriptionParserTests
{
    private readonly string _noTagsDescription = "previous location.";
    private readonly string _normalTagsDescription1 = "Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125</c> bonus by <c val=\"#TooltipNumbers\">2</c> seconds.";
    private readonly string _normalTagsDescription2 = "<c val=\"#TooltipQuest\"> Repeatable Quest:</c> Gain<c val=\"#TooltipNumbers\">10</c>";
    private readonly string _extraTagDescription1 = "</w>previous location.";
    private readonly string _extraTagDescription2 = "previous location.</w>";
    private readonly string _extraTagDescription3 = "previous </w>location.";
    private readonly string _extraTagDescription4 = "<li/>previous location.";
    private readonly string _newLineTagDescription1 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _newLineTagDescription2 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/><n/>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _selfCloseTagDescription1 = "<img path=\"sdf\"/>previous location.";
    private readonly string _selfCloseTagDescription2 = "previous<img path=\"sdf\"/>";
    private readonly string _selfCloseTagDescription3 = "previous<img path=\"sdf\"/><c val=\"#TooltipQuest\"> Repeatable Quest:</c>";
    private readonly string _selfCloseTagDescription4 = "previous<c val=\"#TooltipQuest\"> Repeatable Quest:</c><img path=\"sdf\"/>";
    private readonly string _duplicateTagsDescription1 = "<c val=\"#TooltipQuest\"> Repeatable Quest:</c> Gain<c val=\"#TooltipNumbers\">10</c></c>";
    private readonly string _duplicateTagsDescription2 = "<c val=\"#TooltipQuest\"> Repeatable Quest:</c></c> Gain<c val=\"#TooltipNumbers\">10</c></c>";
    private readonly string _spaceTagNormalDescription1 = "Temps de recharge : 20 secondes";
    private readonly string _spaceTagNormalDescription2 = "À distance ";
    private readonly string _spaceTagNormalDescription3 = " À distance";
    private readonly string _spaceTagDescription1 = "Temps de recharge :<sp/>20 secondes";
    private readonly string _spaceTagDescription2 = "À distance<sp/>";
    private readonly string _spaceTagDescription3 = "<sp/>À distance";
    private readonly string _scaleTagDescription1 = "aa ~~0.045~~ bb";

    // Convert newline tags </n> to <n/>
    private readonly string _convertNewLineTagDescription1 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c></n>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _convertNewLineTagDescription1Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _convertNewLineTagDescription2 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c></n></n>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _convertNewLineTagDescription2Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/><n/>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _convertNewLineTagDescription3 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c></n></n>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c></n>";
    private readonly string _convertNewLineTagDescription3Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/><n/>Health Per Second Bonus: <c val=\"#TooltipNumbers\">0</c><n/>";

    // Case tags
    private readonly string _upperCaseTagDescription1 = "<C val=\"#TooltipQuest\"> Repeatable Quest:</C> Gain<C val=\"#TooltipNumbers\">10</c>";
    private readonly string _upperCaseTagDescription1Corrected = "<c val=\"#TooltipQuest\"> Repeatable Quest:</c> Gain<c val=\"#TooltipNumbers\">10</c>";

    // space in tags
    private readonly string _extraSpacesTagDescription1 = "<c  val=\"#TooltipQuest\"> Repeatable Quest:</c> Gain<c val=\"#TooltipNumbers\">10</c>";
    private readonly string _extraSpacesTagDescription2 = "<c     val=\"#TooltipQuest\"> Repeatable Quest:</c> Gain<c val=\"#TooltipNumbers\">10</c>";

    // Empty text tags
    private readonly string _emptyTagsDescription1 = "<c val=\"#TooltipQuest\"></c><c val=\"#TooltipNumbers\"></c>";
    private readonly string _emptyTagsDescription1Corrected = string.Empty;
    private readonly string _emptyTagsDescription2 = "test1<c val=\"#TooltipQuest\">test2</c>test3 <c val=\"#TooltipNumbers\"></c>";
    private readonly string _emptyTagsDescription2Corrected = "test1<c val=\"#TooltipQuest\">test2</c>test3 ";
    private readonly string _emptyTagsDescription3 = "<c val=\"#TooltipQuest\"></C>test1<c val=\"#TooltipQuest\">test2</c>test3 <c val=\"#TooltipNumbers\"></c>";
    private readonly string _emptyTagsDescription3Corrected = "test1<c val=\"#TooltipQuest\">test2</c>test3 ";

    // nested tags
    private readonly string _nestedTagDescription1 = "<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30%</c> points</c>";
    private readonly string _nestedTagDescription1Corrected = "<c val=\"FF8000\">Gain </c><c val=\"#TooltipNumbers\">30%</c><c val=\"FF8000\"> points</c>";
    private readonly string _nestedTagDescription2 = "<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30%</c> points <c val=\"#TooltipNumbers\">30%</c> charges</c>";
    private readonly string _nestedTagDescription2Corrected = "<c val=\"FF8000\">Gain </c><c val=\"#TooltipNumbers\">30%</c><c val=\"FF8000\"> points </c><c val=\"#TooltipNumbers\">30%</c><c val=\"FF8000\"> charges</c>";
    private readonly string _nestedTagDescription3 = "<c val=\"FF8000\"><c val=\"#TooltipNumbers\"></c></c>";
    private readonly string _nestedTagDescription3Corrected = string.Empty;
    private readonly string _nestedTagDescription4 = "<c val=\"FF8000\">45%<c val=\"#TooltipNumbers\"></c></c>";
    private readonly string _nestedTagDescription4Corrected = "<c val=\"FF8000\">45%</c>";

    // nested new line
    private readonly string _nestedNewLineTagDescription1 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%<n/>5%</c>Health <c val=\"#TooltipNumbers\">0</c>"; // new line between c tags
    private readonly string _nestedNewLineTagDescription1Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/><c val=\"#TooltipNumbers\">5%</c>Health <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _nestedNewLineTagDescription2 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%<n/></c>Health <c val=\"#TooltipNumbers\">0</c>";
    private readonly string _nestedNewLineTagDescription2Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%</c><n/>Health <c val=\"#TooltipNumbers\">0</c>";

    // broken tags
    private readonly string _incompleteTagDescription1 = "Max Health Bonus: <c val=\"#TooltipNumbers\"0%</c>";
    private readonly string _incompleteTagDescription1Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\"0%"; // no start (valid) start tag, only self closing tag so it gets removed
    private readonly string _incompleteTagDescription2 = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%/c>";
    private readonly string _incompleteTagDescription2Corrected = "Max Health Bonus: <c val=\"#TooltipNumbers\">0%/c></c>"; // broken end tag, so add the end tag to close the start tag
    private readonly string _incompleteTagDescription3 = "previous <w>location.";
    private readonly string _incompleteTagDescription3Corrected = "previous <w>location.</w>";
    private readonly string _incompleteTagDescription4 = "previous <w><a>location.";
    private readonly string _incompleteTagDescription4Corrected = "previous <a>location.</a>";
    private readonly string _incompleteTagDescription5 = "previous <w>test<a>location.<";
    private readonly string _incompleteTagDescription5Corrected = "previous <w>test</w><a>location.<</a>";

    // real descriptions
    private readonly string _diabloBlackSoulstone = "<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Repeatable Quest:</c> Gain <c val=\"#TooltipNumbers\">10</c> Souls per Hero killed and <c val=\"#TooltipNumbers\">1</c> Soul per Minion, up to <c val=\"#TooltipNumbers\">100</c>. For each Soul, gain <c val=\"#TooltipNumbers\">0.4%</w></c> maximum Health. If Diablo has <c val=\"#TooltipNumbers\">100</c> Souls upon dying, he will resurrect in <c val=\"#TooltipNumbers\">5</c> seconds but lose <c val=\"#TooltipNumbers\">100</c> Souls.";
    private readonly string _diabloBlackSoulstoneCorrected = "<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Repeatable Quest:</c> Gain <c val=\"#TooltipNumbers\">10</c> Souls per Hero killed and <c val=\"#TooltipNumbers\">1</c> Soul per Minion, up to <c val=\"#TooltipNumbers\">100</c>. For each Soul, gain <c val=\"#TooltipNumbers\">0.4%</c> maximum Health. If Diablo has <c val=\"#TooltipNumbers\">100</c> Souls upon dying, he will resurrect in <c val=\"#TooltipNumbers\">5</c> seconds but lose <c val=\"#TooltipNumbers\">100</c> Souls.";
    private readonly string _dvaMechSelfDestruct = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400</c> to <c val=\"#TooltipNumbers\">1200</c> damage in a large area, depending on distance from center. Only deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">30%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";
    private readonly string _dvaMechSelfDestructCorrected = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400</c> to <c val=\"#TooltipNumbers\">1200</c> damage in a large area, depending on distance from center. Only deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.<n/><n/><c val=\"FF8000\">Gain </c><c val=\"#TooltipNumbers\">1%</c><c val=\"FF8000\"> Charge for every </c><c val=\"#TooltipNumbers\">2</c><c val=\"FF8000\"> seconds spent Basic Attacking, and </c><c val=\"#TooltipNumbers\">30%</c><c val=\"FF8000\"> Charge per </c><c val=\"#TooltipNumbers\">100%</c><c val=\"FF8000\"> of Mech Health lost.</c>";
    private readonly string _valeeraCheapShot = "Deal <c val=\"#TooltipNumbers\">30</c> damage to an enemy, Stun them for <c val=\"#TooltipNumbers\">0.75</c> seconds, and Blind them for <c val=\"#TooltipNumbers\">2</c> seconds once Cheap Shot's Stun expires.<n/><n/><c val=\"#GlowColorRed\">Awards 1 Combo Point.</c><n/><n/><c val=\"#ColorViolet\">Unstealth: Blade Flurry<n/></c>Deal damage in an area around Valeera.";
    private readonly string _valeeraCheapShotCorrected = "Deal <c val=\"#TooltipNumbers\">30</c> damage to an enemy, Stun them for <c val=\"#TooltipNumbers\">0.75</c> seconds, and Blind them for <c val=\"#TooltipNumbers\">2</c> seconds once Cheap Shot's Stun expires.<n/><n/><c val=\"#GlowColorRed\">Awards 1 Combo Point.</c><n/><n/><c val=\"#ColorViolet\">Unstealth: Blade Flurry</c><n/>Deal damage in an area around Valeera.";
    private readonly string _crusaderPunish = "Step forward dealing <c val=\"#TooltipNumbers\">113</c> damage and Slowing enemies by <c val=\"#TooltipNumbers\">60%</c> decaying over <c val=\"#TooltipNumbers\">2</c> seconds.";
    private readonly string _crusaderPunishSame = "Step forward dealing <c val=\"#TooltipNumbers\">113</c> damage and Slowing enemies by <c val=\"#TooltipNumbers\">60%</c> decaying over <c val=\"#TooltipNumbers\">2</c> seconds.";
    private readonly string _meiSnowBlindDeDE = "Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70~~0.045~~</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.";
    private readonly string _meiSnowBlindDeDECorrected = "Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70~~0.045~~</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.";

    // plain text
    private readonly string _plainText1 = "Gain 30% points"; // NestedTagDescription1
    private readonly string _plainText2 = "Max Health Bonus: 0% Health 0"; // NestedNewLineTagDescription2Corrected
    private readonly string _plainText3 = "Deal 30 damage to an enemy, Stun them for 0.75 seconds, and Blind them for 2 seconds once Cheap Shot's Stun expires.  Awards 1 Combo Point.  Unstealth: Blade Flurry Deal damage in an area around Valeera."; // ValeeraCheapShotCorrected
    private readonly string _plainText4 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c> damage per second<n/>";
    private readonly string _plainText4Corrected = "100 damage per second ";
    private readonly string _plainText5 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c> damage per second ~~0.05~~";
    private readonly string _plainText5Corrected = "100 damage per second ";
    private readonly string _plainText6 = "<c val=\"#TooltipNumbers\">100~~no-scale~~</c> damage per second ~~0.05~~";
    private readonly string _plainText6Corrected = "100~~no-scale~~ damage per second ";
    private readonly string _plainTextMeiSnowBlindDeDECorrected = "Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern 70 Schaden zu, verlangsamt sie um 35% und blendet sie 1,75 Sek. lang.";

    // plain text with newlines
    private readonly string _plainTextNewline1 = "Max Health Bonus: 0%<n/>5%Health 0"; // NestedNewLineTagDescription1Corrected
    private readonly string _plainTextNewline2 = "Deal 30 damage to an enemy, Stun them for 0.75 seconds, and Blind them for 2 seconds once Cheap Shot's Stun expires.<n/><n/>Awards 1 Combo Point.<n/><n/>Unstealth: Blade Flurry<n/>Deal damage in an area around Valeera."; // ValeeraCheapShotCorrected
    private readonly string _plainTextNewline3 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c><n/> damage per second ~~0.05~~";
    private readonly string _plainTextNewline3Corrected = "100<n/> damage per second ";

    // plain text with scaling
    private readonly string _plainTextScaling1 = "<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second";
    private readonly string _plainTextScaling1Corrected = "120 (+4% per level)  damage per second";
    private readonly string _plainTextScaling2 = "<c val=\"#TooltipNumbers\">120~~0.05~~</c> damage per second ~~0.035~~<n/>";
    private readonly string _plainTextScaling2Corrected = "120 (+5% per level) damage per second  (+3.5% per level) ";
    private readonly string _plainTextScalingMeiSnowBlindDeDECorrected = "Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern 70 (+4,5% pro Stufe) Schaden zu, verlangsamt sie um 35% und blendet sie 1,75 Sek. lang.";

    // plain text with scaling newlines
    private readonly string _plainTextScalingNewline1 = "<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second";
    private readonly string _plainTextScalingNewline1Corrected = "120 (+4% per level)<n/> damage per second";
    private readonly string _plainTextScalingNewline2 = "<c val=\"#TooltipNumbers\">120~~0.05~~</c> damage per <n/> second ~~0.035~~";
    private readonly string _plainTextScalingNewline2Corrected = "120 (+5% per level) damage per <n/> second  (+3.5% per level)";

    // colored text
    private readonly string _coloredText1 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c><n/> damage per second<n/>";
    private readonly string _coloredText1Corrected = "<c val=\"#TooltipNumbers\">100</c><n/> damage per second<n/>";
    private readonly string _coloredText2 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c> damage per second ~~0.05~~";
    private readonly string _coloredText2Corrected = "<c val=\"#TooltipNumbers\">100</c> damage per second ";

    // colored text with scaling
    private readonly string _coloredTextScaling1 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c><n/> damage per second<n/>";
    private readonly string _coloredTextScaling1Corrected = "<c val=\"#TooltipNumbers\">100 (+4% per level)</c><n/> damage per second<n/>";
    private readonly string _coloredTextScaling2 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c> damage per second ~~0.05~~";
    private readonly string _coloredTextScaling2Corrected = "<c val=\"#TooltipNumbers\">100 (+4% per level)</c> damage per second  (+5% per level)";
    private readonly string _coloredTextScaling3 = "<c val=\"#TooltipNumbers\">100~~no-scale~~</c> damage per second~~0.05~~";
    private readonly string _coloredTextScaling3Corrected = "<c val=\"#TooltipNumbers\">100~~no-scale~~</c> damage per second (+5% per level)";
    private readonly string _coloredTextScaling4 = "<c val=\"#TooltipNumbers\">100~~0.04</c> damage per second~~0.05~~";
    private readonly string _coloredTextScaling4Corrected = "<c val=\"#TooltipNumbers\">100~~0.04</c> damage per second (+5% per level)";
    private readonly string _coloredTextScaling5 = "<c val=\"#TooltipNumbers\">100~~no-scale~~##ERROR</c> damage per second~~0.05~~";
    private readonly string _coloredTextScaling5Corrected = "<c val=\"#TooltipNumbers\">100~~no-scale~~##ERROR</c> damage per second (+5% per level)";

    // text with error tag
    private readonly string _errorText1 = "<c val=\"#TooltipNumbers\">100##ERROR##~~0.04~~</c> damage per second<n/>";
    private readonly string _errorText1Corrected = "<c val=\"#TooltipNumbers\">100 (+4% per level)</c> damage per second<n/>";
    private readonly string _errorText2 = "<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30##ERROR##%</c> points</c>";
    private readonly string _errorText2Corrected = "Gain 30% points";
    private readonly string _errorText3 = "100##ERROR##<n/> damage per second<n/>";
    private readonly string _errorText3Corrected = "100<n/> damage per second<n/>";

    // no scale tag
    private readonly string _noScaleText1 = "100~~no-scale~~ damage per second";
    private readonly string _noScaleText2 = "100~~0.04 damage per second";
    private readonly string _noScaleText3 = "100 0.04~~ damage per second";
    private readonly string _noScaleText4 = "100~0.04~~ damage per second";
    private readonly string _noScaleText5 = "100~~0.04~ damage per second";

    // no error tag
    private readonly string _noErrorText1 = "100##hello##<n/> damage per second<n/>";
    private readonly string _noErrorText2 = "100 ERROR##<n/> damage per second<n/>";
    private readonly string _noErrorText3 = "100##ERROR<n/> damage per second<n/>";
    private readonly string _noErrorText4 = "100#ERROR##<n/> damage per second<n/>";
    private readonly string _noErrorText5 = "100##ERROR#<n/> damage per second<n/>";

    // other languages
    private readonly string _meiSnowBlindEsES = "Lanzas una bola de nieve que golpea a todos los enemigos en un área. Los enemigos golpeados reciben <c val=\"bfd4fd\">70~~0.045~~</c> de daño, quedan ralentizados un <c val=\"bfd4fd\">35%</c> y cegados durante <c val=\"bfd4fd\">1,75</c> s.";
    private readonly string _meiSnowBlindEsMX = "Lanza una bola de nieve que golpea a todos los enemigos en un área. Los enemigos golpeados reciben <c val=\"bfd4fd\">70~~0.045~~</c> de daño, son ralentizados un <c val=\"bfd4fd\">35%</c> y quedan cegados durante <c val=\"bfd4fd\">1.75</c> segundos.";
    private readonly string _meiSnowBlindFrFR = "Lance une boule de neige qui frappe tous les ennemis dans une zone. Les ennemis touchés subissent <c val=\"bfd4fd\">70~~0.045~~</c> points de dégâts, sont ralentis de <c val=\"bfd4fd\">35%</c> et sont aveuglés pendant <c val=\"bfd4fd\">1,75</c> seconde.";
    private readonly string _meiSnowBlindItIT = "Lancia una palla di neve che colpisce tutti i nemici in un'area. I nemici colpiti subiscono <c val=\"bfd4fd\">70~~0.045~~</c> danni, sono rallentati del <c val=\"bfd4fd\">35%</c> e sono accecati per <c val=\"bfd4fd\">1,75</c> s.";
    private readonly string _meiSnowBlindKoKR = "눈덩이를 던져 대상 지역의 모든 적에게 <c val=\"bfd4fd\">70~~0.045~~</c>의 피해를 주고, <c val=\"bfd4fd\">1.75</c>초 동안 <c val=\"bfd4fd\">35%</c> 느려지고 실명하게 합니다.";
    private readonly string _meiSnowBlindPlPL = "Mei ciska śnieżką, która trafia wszystkich przeciwników na danym obszarze. Trafieni przeciwnicy otrzymują <c val=\"bfd4fd\">70~~0.045~~</c> pkt. obrażeń, zostają spowolnieni o <c val=\"bfd4fd\">35%</c>, a także oślepieni na <c val=\"bfd4fd\">1,75</c> sek.";
    private readonly string _meiSnowBlindPtBR = "Joga uma bola de neve que atinge todos os inimigos na área. Inimigos atingidos recebem <c val=\"bfd4fd\">70~~0.045~~</c> de dano, são desacelerados em <c val=\"bfd4fd\">35%</c> e ficam cegos por <c val=\"bfd4fd\">1,75</c> s.";
    private readonly string _meiSnowBlindRuRU = "Бросает снежок, поражая противников в области действия. Пораженные цели получают <c val=\"bfd4fd\">70~~0.045~~</c> ед. урона, замедляются на <c val=\"bfd4fd\">35%</c> и ослепляются на <c val=\"bfd4fd\">1,75</c> сек.";
    private readonly string _meiSnowBlindZhCN = "投掷一个雪球，击中区域内所有敌人。被击中的敌人受到<c val=\"bfd4fd\">70~~0.045~~</c>点伤害，同时会被减速<c val=\"bfd4fd\">35%</c>并且被致盲，持续<c val=\"bfd4fd\">1.75</c>秒。";
    private readonly string _meiSnowBlindZhTW = "投擲一顆雪球，命中範圍內的所有敵人。命中的敵人受到<c val=\"bfd4fd\">70~~0.045~~</c>點傷害和緩速<c val=\"bfd4fd\">35%</c>，同時會目盲<c val=\"bfd4fd\">1.75</c>秒。";

    private readonly string _meiSnowBlindEsESColorWithScaling = "Lanzas una bola de nieve que golpea a todos los enemigos en un área. Los enemigos golpeados reciben <c val=\"bfd4fd\">70 (+4,5% por nivel)</c> de daño, quedan ralentizados un <c val=\"bfd4fd\">35%</c> y cegados durante <c val=\"bfd4fd\">1,75</c> s.";
    private readonly string _meiSnowBlindEsMXColorWithScaling = "Lanza una bola de nieve que golpea a todos los enemigos en un área. Los enemigos golpeados reciben <c val=\"bfd4fd\">70 (+4.5% por nivel)</c> de daño, son ralentizados un <c val=\"bfd4fd\">35%</c> y quedan cegados durante <c val=\"bfd4fd\">1.75</c> segundos.";
    private readonly string _meiSnowBlindFrFRColorWithScaling = "Lance une boule de neige qui frappe tous les ennemis dans une zone. Les ennemis touchés subissent <c val=\"bfd4fd\">70 (+4,5% par niveau)</c> points de dégâts, sont ralentis de <c val=\"bfd4fd\">35%</c> et sont aveuglés pendant <c val=\"bfd4fd\">1,75</c> seconde.";
    private readonly string _meiSnowBlindItITColorWithScaling = "Lancia una palla di neve che colpisce tutti i nemici in un'area. I nemici colpiti subiscono <c val=\"bfd4fd\">70 (+4,5% per livello)</c> danni, sono rallentati del <c val=\"bfd4fd\">35%</c> e sono accecati per <c val=\"bfd4fd\">1,75</c> s.";
    private readonly string _meiSnowBlindKoKRColorWithScaling = "눈덩이를 던져 대상 지역의 모든 적에게 <c val=\"bfd4fd\">70 (레벨당 +4.5%)</c>의 피해를 주고, <c val=\"bfd4fd\">1.75</c>초 동안 <c val=\"bfd4fd\">35%</c> 느려지고 실명하게 합니다.";
    private readonly string _meiSnowBlindPlPLColorWithScaling = "Mei ciska śnieżką, która trafia wszystkich przeciwników na danym obszarze. Trafieni przeciwnicy otrzymują <c val=\"bfd4fd\">70 (+4,5% na poziom)</c> pkt. obrażeń, zostają spowolnieni o <c val=\"bfd4fd\">35%</c>, a także oślepieni na <c val=\"bfd4fd\">1,75</c> sek.";
    private readonly string _meiSnowBlindPtBRColorWithScaling = "Joga uma bola de neve que atinge todos os inimigos na área. Inimigos atingidos recebem <c val=\"bfd4fd\">70 (+4,5% por nível)</c> de dano, são desacelerados em <c val=\"bfd4fd\">35%</c> e ficam cegos por <c val=\"bfd4fd\">1,75</c> s.";
    private readonly string _meiSnowBlindRuRUColorWithScaling = "Бросает снежок, поражая противников в области действия. Пораженные цели получают <c val=\"bfd4fd\">70 (+4,5% за уровень)</c> ед. урона, замедляются на <c val=\"bfd4fd\">35%</c> и ослепляются на <c val=\"bfd4fd\">1,75</c> сек.";
    private readonly string _meiSnowBlindZhCNColorWithScaling = "投掷一个雪球，击中区域内所有敌人。被击中的敌人受到<c val=\"bfd4fd\">70 (每级+4.5%)</c>点伤害，同时会被减速<c val=\"bfd4fd\">35%</c>并且被致盲，持续<c val=\"bfd4fd\">1.75</c>秒。";
    private readonly string _meiSnowBlindZhTWColorWithScaling = "投擲一顆雪球，命中範圍內的所有敵人。命中的敵人受到<c val=\"bfd4fd\">70 (每級+4.5%)</c>點傷害和緩速<c val=\"bfd4fd\">35%</c>，同時會目盲<c val=\"bfd4fd\">1.75</c>秒。";

    [TestMethod]
    public void ValidateTest()
    {
        Assert.AreEqual(_noTagsDescription, DescriptionParser.GetInstance(_noTagsDescription).GetRawDescription()); // no changes
        Assert.AreEqual(_normalTagsDescription1, DescriptionParser.GetInstance(_normalTagsDescription1).GetRawDescription()); // no changes
        Assert.AreEqual(_normalTagsDescription2, DescriptionParser.GetInstance(_normalTagsDescription2).GetRawDescription()); // no changes
        Assert.AreEqual(_noTagsDescription, DescriptionParser.GetInstance(_extraTagDescription1).GetRawDescription());
        Assert.AreEqual(_noTagsDescription, DescriptionParser.GetInstance(_extraTagDescription2).GetRawDescription());
        Assert.AreEqual(_noTagsDescription, DescriptionParser.GetInstance(_extraTagDescription3).GetRawDescription());
        Assert.AreEqual(_noTagsDescription, DescriptionParser.GetInstance(_extraTagDescription4).GetRawDescription());
        Assert.AreEqual(_newLineTagDescription1, DescriptionParser.GetInstance(_newLineTagDescription1).GetRawDescription()); // no changes
        Assert.AreEqual(_newLineTagDescription2, DescriptionParser.GetInstance(_newLineTagDescription2).GetRawDescription()); // no changes
        Assert.AreEqual(_selfCloseTagDescription1, DescriptionParser.GetInstance(_selfCloseTagDescription1).GetRawDescription()); // no changes
        Assert.AreEqual(_selfCloseTagDescription2, DescriptionParser.GetInstance(_selfCloseTagDescription2).GetRawDescription()); // no changes
        Assert.AreEqual(_selfCloseTagDescription3, DescriptionParser.GetInstance(_selfCloseTagDescription3).GetRawDescription()); // no changes
        Assert.AreEqual(_selfCloseTagDescription4, DescriptionParser.GetInstance(_selfCloseTagDescription4).GetRawDescription()); // no changes
        Assert.AreEqual(_normalTagsDescription2, DescriptionParser.GetInstance(_duplicateTagsDescription1).GetRawDescription());
        Assert.AreEqual(_normalTagsDescription2, DescriptionParser.GetInstance(_duplicateTagsDescription2).GetRawDescription());
        Assert.AreEqual(_spaceTagDescription1, DescriptionParser.GetInstance(_spaceTagDescription1).GetRawDescription()); // no changes
        Assert.AreEqual(_spaceTagDescription2, DescriptionParser.GetInstance(_spaceTagDescription2).GetRawDescription()); // no changes
        Assert.AreEqual(_scaleTagDescription1, DescriptionParser.GetInstance(_scaleTagDescription1).GetRawDescription()); // no changes
    }

    [TestMethod]
    public void ValidateBrokenTagsTest()
    {
        Assert.AreEqual(_incompleteTagDescription1Corrected, DescriptionParser.GetInstance(_incompleteTagDescription1).GetRawDescription());
        Assert.AreEqual(_incompleteTagDescription2Corrected, DescriptionParser.GetInstance(_incompleteTagDescription2).GetRawDescription());
        Assert.AreEqual(_incompleteTagDescription3Corrected, DescriptionParser.GetInstance(_incompleteTagDescription3).GetRawDescription());
        Assert.AreEqual(_incompleteTagDescription4Corrected, DescriptionParser.GetInstance(_incompleteTagDescription4).GetRawDescription());
        Assert.AreEqual(_incompleteTagDescription5Corrected, DescriptionParser.GetInstance(_incompleteTagDescription5).GetRawDescription());
    }

    [TestMethod]
    public void ValidateConvertedNewlineTagsTest()
    {
        Assert.AreEqual(_convertNewLineTagDescription1Corrected, DescriptionParser.GetInstance(_convertNewLineTagDescription1).GetRawDescription());
        Assert.AreEqual(_convertNewLineTagDescription2Corrected, DescriptionParser.GetInstance(_convertNewLineTagDescription2).GetRawDescription());
        Assert.AreEqual(_convertNewLineTagDescription3Corrected, DescriptionParser.GetInstance(_convertNewLineTagDescription3).GetRawDescription());
    }

    [TestMethod]
    public void ValidateCaseTagsTest()
    {
        Assert.AreEqual(_upperCaseTagDescription1Corrected, DescriptionParser.GetInstance(_upperCaseTagDescription1).GetRawDescription());
    }

    [TestMethod]
    public void ValidateExtraSpaceInTagsTest()
    {
        Assert.AreEqual(_normalTagsDescription2, DescriptionParser.GetInstance(_extraSpacesTagDescription1).GetRawDescription());
        Assert.AreEqual(_normalTagsDescription2, DescriptionParser.GetInstance(_extraSpacesTagDescription2).GetRawDescription());
    }

    [TestMethod]
    public void ValidateEmptyTagsTest()
    {
        Assert.AreEqual(_emptyTagsDescription1Corrected, DescriptionParser.GetInstance(_emptyTagsDescription1).GetRawDescription());
        Assert.AreEqual(_emptyTagsDescription2Corrected, DescriptionParser.GetInstance(_emptyTagsDescription2).GetRawDescription());
        Assert.AreEqual(_emptyTagsDescription3Corrected, DescriptionParser.GetInstance(_emptyTagsDescription3).GetRawDescription());
    }

    [TestMethod]
    public void ValidateNestedTagsTest()
    {
        Assert.AreEqual(_nestedTagDescription1Corrected, DescriptionParser.GetInstance(_nestedTagDescription1).GetRawDescription());
        Assert.AreEqual(_nestedTagDescription2Corrected, DescriptionParser.GetInstance(_nestedTagDescription2).GetRawDescription());
        Assert.AreEqual(_nestedTagDescription3Corrected, DescriptionParser.GetInstance(_nestedTagDescription3).GetRawDescription());
        Assert.AreEqual(_nestedTagDescription4Corrected, DescriptionParser.GetInstance(_nestedTagDescription4).GetRawDescription());
    }

    [TestMethod]
    public void ValidateNestedNewLineTagsTest()
    {
        Assert.AreEqual(_nestedNewLineTagDescription1Corrected, DescriptionParser.GetInstance(_nestedNewLineTagDescription1).GetRawDescription());
        Assert.AreEqual(_nestedNewLineTagDescription2Corrected, DescriptionParser.GetInstance(_nestedNewLineTagDescription2).GetRawDescription());
    }

    [TestMethod]
    public void ValidateRealDescriptionTest()
    {
        Assert.AreEqual(_diabloBlackSoulstoneCorrected, DescriptionParser.GetInstance(_diabloBlackSoulstone).GetRawDescription());
        Assert.AreEqual(_dvaMechSelfDestructCorrected, DescriptionParser.GetInstance(_dvaMechSelfDestruct).GetRawDescription());
        Assert.AreEqual(_valeeraCheapShotCorrected, DescriptionParser.GetInstance(_valeeraCheapShot).GetRawDescription());
        Assert.AreEqual(_crusaderPunishSame, DescriptionParser.GetInstance(_crusaderPunish).GetRawDescription());
        Assert.AreEqual(_meiSnowBlindDeDECorrected, DescriptionParser.GetInstance(_meiSnowBlindDeDE, StormLocale.DEDE).GetRawDescription());
    }

    [TestMethod]
    public void ValidatePlainTextTest()
    {
        Assert.AreEqual(_plainText1, DescriptionParser.GetInstance(_nestedTagDescription1).GetPlainText(false, false));
        Assert.AreEqual(_plainText2, DescriptionParser.GetInstance(_nestedNewLineTagDescription2Corrected).GetPlainText(false, false));
        Assert.AreEqual(_plainText3, DescriptionParser.GetInstance(_valeeraCheapShotCorrected).GetPlainText(false, false));
        Assert.AreEqual(_plainText4Corrected, DescriptionParser.GetInstance(_plainText4).GetPlainText(false, false));
        Assert.AreEqual(_plainText5Corrected, DescriptionParser.GetInstance(_plainText5).GetPlainText(false, false));
        Assert.AreEqual(_plainText6Corrected, DescriptionParser.GetInstance(_plainText6).GetPlainText(false, false));
        Assert.AreEqual(_plainTextMeiSnowBlindDeDECorrected, DescriptionParser.GetInstance(_meiSnowBlindDeDE, StormLocale.DEDE).GetPlainText(false, false));
    }

    [TestMethod]
    public void ValidatePlainTextNewlineTest()
    {
        Assert.AreEqual(_plainTextNewline1, DescriptionParser.GetInstance(_nestedNewLineTagDescription1Corrected).GetPlainText(true, false));
        Assert.AreEqual(_plainTextNewline2, DescriptionParser.GetInstance(_valeeraCheapShotCorrected).GetPlainText(true, false));
        Assert.AreEqual(_plainTextNewline3Corrected, DescriptionParser.GetInstance(_plainTextNewline3).GetPlainText(true, false));
    }

    [TestMethod]
    public void ValidatePlainTextScalingTest()
    {
        Assert.AreEqual(_plainTextScaling1Corrected, DescriptionParser.GetInstance(_plainTextScaling1).GetPlainText(false, true));
        Assert.AreEqual(_plainTextScaling2Corrected, DescriptionParser.GetInstance(_plainTextScaling2).GetPlainText(false, true));
        Assert.AreEqual(_plainTextScalingMeiSnowBlindDeDECorrected, DescriptionParser.GetInstance(_meiSnowBlindDeDE, StormLocale.DEDE).GetPlainText(false, true));
    }

    [TestMethod]
    public void ValidatePlainTextScalingNewlineTest()
    {
        Assert.AreEqual(_plainTextScalingNewline1Corrected, DescriptionParser.GetInstance(_plainTextScalingNewline1).GetPlainText(true, true));
        Assert.AreEqual(_plainTextScalingNewline2Corrected, DescriptionParser.GetInstance(_plainTextScalingNewline2).GetPlainText(true, true));
    }

    [TestMethod]
    public void ValidateColoredTextTest()
    {
        Assert.AreEqual(_coloredText1Corrected, DescriptionParser.GetInstance(_coloredText1).GetColoredText(false));
        Assert.AreEqual(_coloredText2Corrected, DescriptionParser.GetInstance(_coloredText2).GetColoredText(false));
    }

    [TestMethod]
    public void ValidateColoredTextScalingTest()
    {
        Assert.AreEqual(_coloredTextScaling1Corrected, DescriptionParser.GetInstance(_coloredTextScaling1).GetColoredText(true));
        Assert.AreEqual(_coloredTextScaling2Corrected, DescriptionParser.GetInstance(_coloredTextScaling2).GetColoredText(true));
        Assert.AreEqual(_coloredTextScaling3Corrected, DescriptionParser.GetInstance(_coloredTextScaling3).GetColoredText(true));
        Assert.AreEqual(_coloredTextScaling4Corrected, DescriptionParser.GetInstance(_coloredTextScaling4).GetColoredText(true));
        Assert.AreEqual(_coloredTextScaling5Corrected, DescriptionParser.GetInstance(_coloredTextScaling5).GetColoredText(true));
    }

    [TestMethod]
    public void ValidateErrorTextTest()
    {
        Assert.AreEqual(_errorText1, DescriptionParser.GetInstance(_errorText1).GetRawDescription());
        Assert.AreEqual(_errorText1Corrected, DescriptionParser.GetInstance(_errorText1).GetColoredText(true));
        Assert.AreEqual(_errorText2Corrected, DescriptionParser.GetInstance(_errorText2).GetPlainText(false, false));
        Assert.AreEqual(_errorText3Corrected, DescriptionParser.GetInstance(_errorText3).GetPlainText(true, false));
    }

    [TestMethod]
    public void ValidateSpaceTagsTest()
    {
        Assert.AreEqual(_spaceTagNormalDescription1, DescriptionParser.GetInstance(_spaceTagDescription1).GetColoredText(true));
        Assert.AreEqual(_spaceTagNormalDescription2, DescriptionParser.GetInstance(_spaceTagDescription2).GetColoredText(true));
        Assert.AreEqual(_spaceTagNormalDescription3, DescriptionParser.GetInstance(_spaceTagDescription3).GetColoredText(true));
        Assert.AreEqual(_spaceTagNormalDescription1, DescriptionParser.GetInstance(_spaceTagDescription1).GetPlainText(false, true));
        Assert.AreEqual(_spaceTagNormalDescription2, DescriptionParser.GetInstance(_spaceTagDescription2).GetPlainText(true, true));
        Assert.AreEqual(_spaceTagNormalDescription3, DescriptionParser.GetInstance(_spaceTagDescription3).GetPlainText(true, true));
    }

    [TestMethod]
    public void LanguageTests()
    {
        Assert.AreEqual(_meiSnowBlindEsESColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindEsES, StormLocale.ESES).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindEsMXColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindEsMX, StormLocale.ESMX).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindFrFRColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindFrFR, StormLocale.FRFR).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindItITColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindItIT, StormLocale.ITIT).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindKoKRColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindKoKR, StormLocale.KOKR).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindPlPLColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindPlPL, StormLocale.PLPL).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindPtBRColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindPtBR, StormLocale.PTBR).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindRuRUColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindRuRU, StormLocale.RURU).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindZhCNColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindZhCN, StormLocale.ZHCN).GetColoredText(true));
        Assert.AreEqual(_meiSnowBlindZhTWColorWithScaling, DescriptionParser.GetInstance(_meiSnowBlindZhTW, StormLocale.ZHTW).GetColoredText(true));
    }

    [TestMethod]
    public void NoScaleTagTests()
    {
        Assert.AreEqual(_noScaleText1, DescriptionParser.GetInstance(_noScaleText1).GetColoredText(true));
        Assert.AreEqual(_noScaleText2, DescriptionParser.GetInstance(_noScaleText2).GetColoredText(true));
        Assert.AreEqual(_noScaleText3, DescriptionParser.GetInstance(_noScaleText3).GetColoredText(true));
        Assert.AreEqual(_noScaleText4, DescriptionParser.GetInstance(_noScaleText4).GetColoredText(true));
        Assert.AreEqual(_noScaleText5, DescriptionParser.GetInstance(_noScaleText5).GetColoredText(true));

        Assert.AreEqual(_noScaleText1, DescriptionParser.GetInstance(_noScaleText1).GetPlainText(false, true));
        Assert.AreEqual(_noScaleText2, DescriptionParser.GetInstance(_noScaleText2).GetPlainText(false, true));
        Assert.AreEqual(_noScaleText3, DescriptionParser.GetInstance(_noScaleText3).GetPlainText(false, true));
        Assert.AreEqual(_noScaleText4, DescriptionParser.GetInstance(_noScaleText4).GetPlainText(false, true));
        Assert.AreEqual(_noScaleText5, DescriptionParser.GetInstance(_noScaleText5).GetPlainText(false, true));
    }

    [TestMethod]
    public void NoErrorTagsTests()
    {
        Assert.AreEqual(_noErrorText1, DescriptionParser.GetInstance(_noErrorText1).GetRawDescription());
        Assert.AreEqual(_noErrorText2, DescriptionParser.GetInstance(_noErrorText2).GetRawDescription());
        Assert.AreEqual(_noErrorText3, DescriptionParser.GetInstance(_noErrorText3).GetRawDescription());
        Assert.AreEqual(_noErrorText4, DescriptionParser.GetInstance(_noErrorText4).GetRawDescription());
        Assert.AreEqual(_noErrorText5, DescriptionParser.GetInstance(_noErrorText5).GetRawDescription());

        Assert.AreEqual(_noErrorText1, DescriptionParser.GetInstance(_noErrorText1).GetPlainText(true, false));
        Assert.AreEqual(_noErrorText2, DescriptionParser.GetInstance(_noErrorText2).GetPlainText(true, false));
        Assert.AreEqual(_noErrorText3, DescriptionParser.GetInstance(_noErrorText3).GetPlainText(true, false));
        Assert.AreEqual(_noErrorText4, DescriptionParser.GetInstance(_noErrorText4).GetPlainText(true, false));
        Assert.AreEqual(_noErrorText5, DescriptionParser.GetInstance(_noErrorText5).GetPlainText(true, false));
    }
}
