using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TMPro
{
    public class TMP_EmojiTextUGUI : TextMeshProUGUI
    {
        #region Private Fields

        protected bool m_emojiParsingRequired = true;

        #endregion

        #region Emoji Parser Functions

        protected virtual bool ParseInputTextAndEmojiCharSequence()
        {
            m_emojiParsingRequired = false;

            //Only parse when richtext active (we need the <sprite=index> tag)
            if (m_isRichText)
            {
                m_emojiParsingRequired = false;
                m_isCalculateSizeRequired = true;

                var v_parsedEmoji = false;
                var v_oldText = m_text;
                v_parsedEmoji = TMP_EmojiSearchEngine.ParseEmojiCharSequence(spriteAsset, ref m_text);
                ParseInputText();

                //We must revert the original text because we dont want to permanently change the text
                m_text = v_oldText;

                return v_parsedEmoji;
            }

            return false;
        }
        
        #endregion

        #region Text Overriden Functions

        public override void SetVerticesDirty()
        {
            //In textmeshpro 1.4 the parameter "m_isInputParsingRequired" changed to internal, so, to dont use reflection i changed to "m_havePropertiesChanged" parameter
#if UNITY_2018_3_OR_NEWER
            if (m_havePropertiesChanged)
#else
            if (m_isInputParsingRequired)
#endif
            {
                m_emojiParsingRequired = m_isRichText;
            }
            base.SetVerticesDirty();
        }

        public override void Rebuild(CanvasUpdate update)
        {
            if (this == null) return;

            if (m_emojiParsingRequired)
                ParseInputTextAndEmojiCharSequence();

            base.Rebuild(update);
        }

        public override string GetParsedText()
        {
            if (m_emojiParsingRequired)
                ParseInputTextAndEmojiCharSequence();

            return base.GetParsedText();
        }

        public override TMP_TextInfo GetTextInfo(string text)
        {
            TMP_EmojiSearchEngine.ParseEmojiCharSequence(spriteAsset, ref text);
            return base.GetTextInfo(text);
        }

        protected override Vector2 CalculatePreferredValues(float defaultFontSize, Vector2 marginSize, bool ignoreTextAutoSizing)
        {
            if (m_emojiParsingRequired)
                ParseInputTextAndEmojiCharSequence();

            return base.CalculatePreferredValues(defaultFontSize, marginSize, ignoreTextAutoSizing);
        }

        #endregion
    }
}
