using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using PoeFilterParser.Model;
using PoEHUD.Controllers;
using PoEHUD.Models.Enums;
using PoEHUD.Models.Interfaces;
using PoEHUD.PoE.Components;
using SharpDX;

namespace PoEHUD.HUD.Loot
{
    public class PoEFilterVisitor : PoeFilterBaseVisitor<AlertDrawStyle>
    {
        private readonly GameController gameController;
        private readonly IParseTree tree;
        private readonly ItemAlertSettings settings;
        private IEntity entity;

        public PoEFilterVisitor(IParseTree tree, GameController gameController, ItemAlertSettings settings)
        {
            this.tree = tree;
            this.gameController = gameController;
            this.settings = settings;
        }

        public AlertDrawStyle Visit(IEntity entity)
        {
            this.entity = entity;
            return base.Visit(tree);
        }

        public override AlertDrawStyle VisitMain(PoeFilterParser.Model.PoeFilterParser.MainContext context)
        {
            if (entity == null || gameController?.Files?.BaseItemTypes == null)
            {
                return null;
            }

            bool filterEnabled = settings.WithBorder || settings.WithSound;
            var baseItemType = gameController.Files.BaseItemTypes.Translate(entity.Path);
            if (baseItemType == null)
            {
                return null;
            }

            string basename = baseItemType.BaseName;
            int dropLevel = baseItemType.DropLevel;
            Models.ItemClass tmp;
            string className = gameController.Files.ItemClasses.Contents.TryGetValue(baseItemType.ClassName, out tmp) ? tmp.ClassName : baseItemType.ClassName;
            var itemBase = entity.GetComponent<Base>();
            int width = baseItemType.Width;
            int height = baseItemType.Height;
            var blocks = context.block();
            var mods = entity.GetComponent<Mods>();
            bool isSkillHGem = entity.HasComponent<SkillGem>();
            bool isMap = entity.HasComponent<Map>();
            var itemRarity = mods.ItemRarity;
            int quality = 0;
            if (entity.HasComponent<Quality>())
            {
                quality = entity.GetComponent<Quality>().ItemQuality;
            }

            string text = string.Concat(quality > 0 ? "Superior " : string.Empty, basename);
            var sockets = entity.GetComponent<Sockets>();
            int numberOfSockets = sockets.NumberOfSockets;
            int largestLinkSize = sockets.LargestLinkSize;
            List<string> socketGroup = sockets.SocketGroup;
            string path = entity.Path;

            Color defaultTextColor;
            //// if (basename.Contains("Portal") || basename.Contains("Wisdom")) { return null; }
            if (basename.Contains("Scroll"))
            {
                return null;
            }

            if (path.Contains("Currency"))
            {
                defaultTextColor = HUDSkin.CurrencyColor;
            }
            else if (path.Contains("DivinationCards"))
            {
                defaultTextColor = HUDSkin.DivinationCardColor;
            }
            else if (path.Contains("Talismans"))
            {
                defaultTextColor = HUDSkin.TalismanColor;
            }
            else if (isSkillHGem)
            {
                defaultTextColor = HUDSkin.SkillGemColor;
            }
            else
            {
                defaultTextColor = AlertDrawStyle.GetTextColorByRarity(itemRarity);
            }

            int defaultBorderWidth = isMap || path.Contains("VaalFragment") ? 1 : 0;

            foreach (var block in blocks)
            {
                var isShow = block.visibility().SHOW() != null;
                bool itemLevelCondition = true;
                bool dropLevelCondition = true;
                bool poeClassCondition = true;
                bool poeBaseTypeCondition = true;
                bool poeRarityCondition = true;
                bool poeQualityCondition = true;
                bool poeWidthCondition = true;
                bool poeHeightCondition = true;
                bool poeSocketsCondition = true;
                bool poeLinkedSocketsCondition = true;
                bool poeSocketGroupCondition = true;
                bool poeIdentifiedCondition = true;
                bool poeCorruptedCondition = true;
                var backgroundColor = AlertDrawStyle.DefaultBackgroundColor;
                var borderColor = Color.White;
                var textColor = defaultTextColor;
                int borderWidth = defaultBorderWidth;
                int sound = -1;
                var statements = block.statement();

                foreach (var statement in statements)
                {
                    var poeItemLevelContext = statement.poeItemLevel();
                    if (poeItemLevelContext != null)
                    {
                        itemLevelCondition &= CalculateDigitsCondition(poeItemLevelContext.compareOpNullable(), poeItemLevelContext.digitsParams(), mods.ItemLevel);
                    }
                    else
                    {
                        var poeDropLevelContext = statement.poeDropLevel();
                        if (poeDropLevelContext != null)
                        {
                            dropLevelCondition &= CalculateDigitsCondition(poeDropLevelContext.compareOpNullable(), poeDropLevelContext.digitsParams(), dropLevel);
                        }
                        else
                        {
                            var poeClassContext = statement.poeClass();
                            if (poeClassContext != null)
                            {
                                poeClassCondition = poeClassContext.@params()
                                    .strValue().Any(y => className.Contains(GetRawText(y)));
                            }
                            else
                            {
                                var poeBaseTypeContext = statement.poeBaseType();
                                if (poeBaseTypeContext != null)
                                {
                                    poeBaseTypeCondition = poeBaseTypeContext.@params()
                                        .strValue().Any(y => basename.Contains(GetRawText(y)));
                                }
                                else
                                {
                                    var poeRarityContext = statement.poeRarity();
                                    if (poeRarityContext != null)
                                    {
                                        Func<int, int, bool> compareFunc = OpConvertor(poeRarityContext.compareOpNullable());
                                        poeRarityCondition &= poeRarityContext.rariryParams().rarityValue().Any(y =>
                                        {
                                            ItemRarity poeItemRarity;
                                            Enum.TryParse(GetRawText(y), true, out poeItemRarity);
                                            return compareFunc((int)itemRarity, (int)poeItemRarity);
                                        });
                                    }
                                    else
                                    {
                                        var poeQualityContext = statement.poeQuality();
                                        if (poeQualityContext != null)
                                        {
                                            poeQualityCondition &= CalculateDigitsCondition(poeQualityContext.compareOpNullable(), poeQualityContext.digitsParams(), quality);
                                        }
                                        else
                                        {
                                            var poeWidthContext = statement.poeWidth();
                                            if (poeWidthContext != null)
                                            {
                                                poeWidthCondition &= CalculateDigitsCondition(poeWidthContext.compareOpNullable(), poeWidthContext.digitsParams(), width);
                                            }
                                            else
                                            {
                                                var poeHeightContext = statement.poeHeight();
                                                if (poeHeightContext != null)
                                                {
                                                    poeHeightCondition &= CalculateDigitsCondition(poeHeightContext.compareOpNullable(), poeHeightContext.digitsParams(), height);
                                                }
                                                else
                                                {
                                                    var poeSocketsContext = statement.poeSockets();
                                                    if (poeSocketsContext != null)
                                                    {
                                                        poeSocketsCondition &= CalculateDigitsCondition(poeSocketsContext.compareOpNullable(), poeSocketsContext.digitsParams(), numberOfSockets);
                                                    }
                                                    else
                                                    {
                                                        var poeLinkedSocketsContext = statement.poeLinkedSockets();
                                                        if (poeLinkedSocketsContext != null)
                                                        {
                                                            poeLinkedSocketsCondition &= CalculateDigitsCondition(poeLinkedSocketsContext.compareOpNullable(), poeLinkedSocketsContext.digitsParams(), largestLinkSize);
                                                        }
                                                        else
                                                        {
                                                            var poeSocketGroupContext = statement.poeSocketGroup();
                                                            if (poeSocketGroupContext != null)
                                                            {
                                                                poeSocketGroupCondition &= poeSocketGroupContext.socketParams().socketValue().Any(y =>
                                                                {
                                                                    string poeSocketGroup = GetRawText(y);
                                                                    return IsContainSocketGroup(socketGroup, poeSocketGroup);
                                                                });
                                                            }
                                                            else
                                                            {
                                                                var poeBackgroundColorContext = statement.poeBackgroundColor();
                                                                if (poeBackgroundColorContext != null)
                                                                {
                                                                    backgroundColor = ToColor(poeBackgroundColorContext.color());
                                                                }
                                                                else
                                                                {
                                                                    var poeBorderColorContext = statement.poeBorderColor();
                                                                    if (poeBorderColorContext != null)
                                                                    {
                                                                        borderColor = ToColor(poeBorderColorContext.color());
                                                                        borderWidth = borderColor.A == 0 ? 0 : 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        var poeTextColorContext = statement.poeTextColor();
                                                                        if (poeTextColorContext != null)
                                                                        {
                                                                            textColor = ToColor(poeTextColorContext.color());
                                                                        }
                                                                        else
                                                                        {
                                                                            var poeAlertSoundContext = statement.poeAlertSound();
                                                                            if (poeAlertSoundContext != null)
                                                                            {
                                                                                sound = Convert.ToInt32(poeAlertSoundContext.id().GetText());
                                                                            }
                                                                            else
                                                                            {
                                                                                var poeIdentifiedContext = statement.poeIdentified();
                                                                                if (poeIdentifiedContext != null)
                                                                                {
                                                                                    var valFromFilter = Convert.ToBoolean(poeIdentifiedContext.Boolean().GetText());
                                                                                    poeIdentifiedCondition &= itemRarity != ItemRarity.Normal ? valFromFilter == mods.Identified : valFromFilter == false;
                                                                                }
                                                                                else
                                                                                {
                                                                                    var poeCorruptedContext = statement.poeCorrupted();
                                                                                    if (poeCorruptedContext != null)
                                                                                    {
                                                                                        poeCorruptedCondition &= itemBase.IsCorrupted == Convert.ToBoolean(poeCorruptedContext.Boolean().GetText());
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!itemLevelCondition || !dropLevelCondition || !poeClassCondition || !poeBaseTypeCondition
                    || !poeRarityCondition || !poeQualityCondition || !poeWidthCondition || !poeHeightCondition
                    || !poeSocketsCondition || !poeLinkedSocketsCondition || !poeSocketGroupCondition
                    || !poeIdentifiedCondition || !poeCorruptedCondition)
                {
                    continue;
                }

                if (!isShow || filterEnabled && !(settings.WithBorder && borderWidth > 0 || settings.WithSound && sound >= 0))
                {
                    return null;
                }

                int iconIndex;
                if (largestLinkSize == 6)
                {
                    iconIndex = 3;
                }
                else if (numberOfSockets == 6)
                {
                    iconIndex = 0;
                }
                else if (IsContainSocketGroup(socketGroup, "RGB"))
                {
                    iconIndex = 1;
                }
                else
                {
                    iconIndex = -1;
                }

                return new AlertDrawStyle(text, textColor, borderWidth, borderColor, backgroundColor, iconIndex);
            }

            return null;
        }

        private static string GetRawText(IParseTree context)
        {
            return context.GetText().Trim('"');
        }

        private static bool IsContainSocketGroup(IEnumerable<string> socketGroup, string str)
        {
            str = string.Concat(str.OrderBy(y => y));
            return socketGroup.Select(group => string.Concat(group.OrderBy(y => y))).Any(sortedGroup => sortedGroup.Contains(str));
        }

        private static Color ToColor(PoeFilterParser.Model.PoeFilterParser.ColorContext colorContext)
        {
            var alphaContext = colorContext.alpha();
            int alpha = alphaContext.DIGITS() != null ? Convert.ToByte(alphaContext.GetText()) : 255;
            byte red = Convert.ToByte(colorContext.red().GetText());
            byte green = Convert.ToByte(colorContext.green().GetText());
            byte blue = Convert.ToByte(colorContext.blue().GetText());
            return new Color(red, green, blue, alpha);
        }

        private static bool CalculateDigitsCondition(PoeFilterParser.Model.PoeFilterParser.CompareOpNullableContext compareOpNullable, PoeFilterParser.Model.PoeFilterParser.DigitsParamsContext digitsParams, int value)
        {
            Func<int, int, bool> compareFunc = OpConvertor(compareOpNullable);
            return digitsParams.DIGITS().Any(y =>
            {
                int poeValue = Convert.ToInt32(y.GetText());
                return compareFunc(value, poeValue);
            });
        }

        private static Func<int, int, bool> OpConvertor(PoeFilterParser.Model.PoeFilterParser.CompareOpNullableContext terminalnode)
        {
            string text = terminalnode.COMPAREOP()?.GetText() ?? "=";
            switch (text)
            {
                case "=": return (x, y) => x == y;
                case "<": return (x, y) => x < y;
                case ">": return (x, y) => x > y;
                case "<=": return (x, y) => x <= y;
                case ">=": return (x, y) => x >= y;
            }

            throw new ArrayTypeMismatchException();
        }
    }
}
