// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.HtmlConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class HtmlConverter
  {
    private static string RemoveInvalidXmlChars(string text)
    {
      return new string(text.Where<char>((Func<char, bool>) (ch => XmlConvert.IsXmlChar(ch))).ToArray<char>());
    }

    public static string ConvertToPlainText(string html)
    {
      if (string.IsNullOrEmpty(html))
        return "";
      html = HtmlConverter.RemoveInvalidXmlChars(html);
      html = "<html>" + html + "</html>";
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(html);
      StringWriter outText = new StringWriter();
      foreach (XmlNode childNode in xmlDocument.ChildNodes)
        HtmlConverter.ConvertTo(childNode, (TextWriter) outText);
      outText.Flush();
      return outText.ToString();
    }

    private static void ConvertTo(XmlNode node, TextWriter outText)
    {
      switch (node.NodeType)
      {
        case XmlNodeType.Element:
          switch (node.Name)
          {
            case "p":
              outText.Write("\r\n");
              break;
            case "br":
              outText.Write("\r\n");
              break;
          }
          if (!node.HasChildNodes)
            break;
          HtmlConverter.ConvertContentTo(node, outText);
          break;
        case XmlNodeType.Text:
          string innerText = node.InnerText;
          if (innerText.Trim().Length <= 0)
            break;
          bool flag = HtmlConverter.IsNodeBehindBr(node);
          if (node.ParentNode?.Name == "li" && !flag)
            outText.Write(HtmlConverter.GetNodeSortLabel(node.ParentNode));
          outText.Write(innerText.Trim());
          if (!(node.ParentNode?.Name == "li") || !HtmlConverter.InFrontOfLiOrIsTheLastChild(node))
            break;
          outText.Write("\r\n");
          break;
        case XmlNodeType.Document:
          HtmlConverter.ConvertContentTo(node, outText);
          break;
      }
    }

    private static bool InFrontOfLiOrIsTheLastChild(XmlNode node)
    {
      if (node.ParentNode != null)
      {
        for (int i = 0; i < node.ParentNode.ChildNodes.Count; ++i)
        {
          if (node.ParentNode.ChildNodes[i] == node && (i < node.ParentNode.ChildNodes.Count - 1 && (node.ParentNode.ChildNodes[i + 1].Name == "ul" || node.ParentNode.ChildNodes[i + 1].Name == "ol") || i == node.ParentNode.ChildNodes.Count - 1))
            return true;
        }
      }
      return false;
    }

    private static bool IsNodeBehindBr(XmlNode node)
    {
      if (node.ParentNode != null)
      {
        for (int i = node.ParentNode.ChildNodes.Count - 1; i >= 0; --i)
        {
          if (node.ParentNode.ChildNodes[i] == node && i > 0 && node.ParentNode.ChildNodes[i - 1].Name == "br")
            return true;
        }
      }
      return false;
    }

    private static void ConvertContentTo(XmlNode node, TextWriter outText)
    {
      foreach (XmlNode childNode in node.ChildNodes)
        HtmlConverter.ConvertTo(childNode, outText);
    }

    private static string GetNodeSortLabel(XmlNode node)
    {
      string nodeSortLabel = "";
      if (node.ParentNode != null)
      {
        int ulOrOlParentCount = HtmlConverter.GetUlOrOlParentCount(node, 0);
        for (int index = 0; index < ulOrOlParentCount - 1; ++index)
          nodeSortLabel += "    ";
        if (node.ParentNode.Name == "ul")
          nodeSortLabel += "- ";
        if (node.ParentNode.Name == "ol")
        {
          int num = 0;
          foreach (XmlNode childNode in node.ParentNode.ChildNodes)
          {
            if (childNode.Name == "li")
              ++num;
            if (childNode == node)
              break;
          }
          nodeSortLabel = nodeSortLabel + num.ToString() + ".";
        }
      }
      return nodeSortLabel;
    }

    private static int GetUlOrOlParentCount(XmlNode node, int num)
    {
      if (node.ParentNode != null && (node.ParentNode.Name == "ol" || node.ParentNode.Name == "ul"))
        ++num;
      return node.ParentNode == null || !(node.ParentNode.Name != "#document") ? num : HtmlConverter.GetUlOrOlParentCount(node.ParentNode, num);
    }

    public static void Html2Rtf(string html)
    {
      WebBrowser webBrowser = new WebBrowser();
      webBrowser.CreateControl();
      webBrowser.DocumentText = HtmlConverter.GetHtml(html);
      webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(HtmlConverter.webBrowser_DocumentCompleted);
    }

    private static string GetHtml(string html)
    {
      html = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"> <title></title><style type=\"text/css\">li{font-size:13px}ul{list-style-type:disc}h1 {font-size:18px}h2 {font-size:16px}h3 {font-size:14px}p {font-size:13px}</style></head><body>" + html + "</body></html>";
      return html;
    }

    private static void webBrowser_DocumentCompleted(
      object sender,
      WebBrowserDocumentCompletedEventArgs e)
    {
      try
      {
        if (!(sender is WebBrowser webBrowser))
          return;
        webBrowser.Document?.ExecCommand("SelectAll", true, (object) null);
        webBrowser.Document?.ExecCommand("Copy", false, (object) null);
      }
      catch (Exception ex)
      {
      }
    }

    public static string MdToHtml(string handledText)
    {
      try
      {
        return CommonMarkConverter.Convert(handledText.Replace("<", "\\<").Replace(">", "\\>"), new CommonMarkSettings()
        {
          RenderSoftLineBreaksAsLineBreaks = true
        });
      }
      catch (Exception ex)
      {
        UtilLog.Warn(ex.Message);
        return (string) null;
      }
    }
  }
}
