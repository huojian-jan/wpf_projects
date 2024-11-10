// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ImageElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.Files;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class ImageElementGenerator : VisualLineElementGenerator
  {
    public static readonly int SmallSizeHeight = 120;
    private const int PrintImageMaxHeight = 800;
    private readonly MarkDownEditor _editor;
    private string _taskId;
    public static HashSet<int> NotImageUrlSet = new HashSet<int>();
    private readonly ConcurrentDictionary<string, FrameworkElement> _cachedControls = new ConcurrentDictionary<string, FrameworkElement>();
    private double _currentWidth;

    private void SetTaskId(object sender, string id)
    {
      if (this._taskId == null || this._taskId != id)
      {
        this._taskId = id;
        ImageElementGenerator.NotImageUrlSet.Clear();
        this._cachedControls.Clear();
      }
      ThemeUtil.TryClearImageCached();
      AttachmentLoadHelper.ClearFailed();
    }

    public ImageElementGenerator(MarkDownEditor editor)
    {
      this._editor = editor;
      editor.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      editor.TaskIdChanged += new EventHandler<string>(this.SetTaskId);
      editor.Unloaded += (RoutedEventHandler) ((s, e) => this._cachedControls.Clear());
    }

    private AttachmentViewModel GetAttachmentViewModel(string id)
    {
      if (string.IsNullOrEmpty(id))
        return (AttachmentViewModel) null;
      return this._cachedControls.ContainsKey(id) && this._cachedControls[id].DataContext is AttachmentViewModel dataContext ? dataContext : (AttachmentViewModel) null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (Math.Abs(e.NewSize.Width - this._currentWidth) <= 1.0)
        return;
      this._editor.Redraw();
      this._currentWidth = e.NewSize.Width;
    }

    private void GetMatch(int startOffset, out int matchOffset)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      Dictionary<int, AttachmentInfo> imageDict = this._editor?.GetImageDict();
      if (imageDict != null && imageDict.Any<KeyValuePair<int, AttachmentInfo>>())
      {
        foreach (KeyValuePair<int, AttachmentInfo> keyValuePair in imageDict)
        {
          if (keyValuePair.Key >= startOffset)
          {
            if (keyValuePair.Key + keyValuePair.Value.Value.Length <= endOffset)
            {
              try
              {
                if (text.Text.Substring(keyValuePair.Key - startOffset + text.Offset, keyValuePair.Value.Value.Length) == keyValuePair.Value.Value)
                {
                  matchOffset = keyValuePair.Key;
                  return;
                }
              }
              catch (Exception ex)
              {
              }
            }
          }
        }
      }
      matchOffset = -1;
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
      int matchOffset;
      this.GetMatch(startOffset, out matchOffset);
      return matchOffset;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      Dictionary<int, AttachmentInfo> imageDict = this._editor.GetImageDict();
      if (!imageDict.ContainsKey(offset))
        return (VisualLineElement) null;
      AttachmentInfo info = imageDict[offset];
      if (endOffset < offset + info.Value.Length)
        return (VisualLineElement) null;
      switch (info.Kind)
      {
        case "image":
          try
          {
            return (VisualLineElement) this.GetImageElement(info);
          }
          catch (Exception ex)
          {
            UtilLog.Error(string.Format("ImageElementGenerator.ConstructElement, GetImageElement Failed {0}", (object) ex));
            return (VisualLineElement) null;
          }
        case "file":
          return (VisualLineElement) this.GetFileElement(info);
        default:
          return (VisualLineElement) null;
      }
    }

    private AttachmentElement GetImageElement(AttachmentInfo info)
    {
      string str1 = info.Url;
      int imageMode = this._editor.ImageMode;
      int length = info.Value.Length;
      int offset = info.Offset;
      AttachmentMDDisplayModel attachmentMdDisplayModel = new AttachmentMDDisplayModel()
      {
        Info = info
      };
      bool flag1 = false;
      string key = string.Empty;
      if (!Uri.IsWellFormedUriString(str1, UriKind.Absolute))
      {
        AttachmentModel attachmentInUrl = AttachmentDao.GetAttachmentInUrl(str1);
        if (attachmentInUrl == null || attachmentInUrl.deleted)
          return (AttachmentElement) null;
        attachmentMdDisplayModel.Attachment = attachmentInUrl;
        if (FileUtils.FileEmptyOrNotExists(attachmentInUrl.localPath))
        {
          if (AttachmentLoadHelper.IsFailed(attachmentInUrl.id))
            flag1 = true;
          else if (!AttachmentLoadHelper.IsLoading(attachmentInUrl.id))
            AttachmentDownloadUtils.AutoDownloadImgAttachment(attachmentInUrl);
        }
        else
        {
          str1 = attachmentInUrl.localPath;
          attachmentMdDisplayModel.LocalPath = attachmentInUrl.localPath;
        }
      }
      else
      {
        string str2 = str1.GetHashCode().ToString();
        if (!Directory.Exists(AppPaths.ImageDir))
          Directory.CreateDirectory(AppPaths.ImageDir);
        string path = ((IEnumerable<string>) Directory.GetFiles(AppPaths.ImageDir, str2 + ".*")).FirstOrDefault<string>();
        if (!string.IsNullOrEmpty(path))
        {
          attachmentMdDisplayModel.LocalPath = path;
          str1 = path;
          key = string.Format("{0}{1}", (object) str1, (object) offset);
        }
        if (FileUtils.FileEmptyOrNotExists(path))
          return (AttachmentElement) null;
      }
      bool flag2 = this._editor.EditBox.SelectionStart <= offset && this._editor.EditBox.SelectionStart + this._editor.EditBox.SelectionLength >= offset + length;
      this._currentWidth = this._editor.ActualWidth != 0.0 || double.IsNaN(this._editor.Width) ? this._editor.ActualWidth : this._editor.Width;
      AttachmentViewModel attachmentViewModel = this.GetAttachmentViewModel(attachmentMdDisplayModel.Attachment?.id ?? key);
      bool flag3 = attachmentViewModel == null;
      if (attachmentViewModel?.AttachmentModel != null && attachmentMdDisplayModel.Attachment != null)
      {
        attachmentViewModel.AttachmentModel = attachmentMdDisplayModel.Attachment;
        attachmentViewModel.FileStatus = AttachmentLoadHelper.AttachmentStatus(attachmentMdDisplayModel.Attachment);
      }
      AttachmentViewModel viewModel = attachmentViewModel ?? (str1.StartsWith(AppPaths.ImageDir) ? new AttachmentViewModel(str1, imageMode) : new AttachmentViewModel(attachmentMdDisplayModel.Attachment, imageMode));
      viewModel.LocalPath = flag1 ? "failed" : str1;
      bool flag4 = imageMode == 1;
      double maxWidth = (this._editor.ActualWidth != 0.0 || double.IsNaN(this._editor.Width) ? this._editor.ActualWidth : this._editor.Width) - 20.0;
      int maxHeight = imageMode == 1 ? ImageElementGenerator.SmallSizeHeight : 15000;
      if (this._editor.InPrint && !flag4)
      {
        maxHeight = 800;
        flag4 = true;
      }
      Size imageSize = ThemeUtil.GetImageSize(str1);
      Size size = flag4 ? ImageUtils.GetSmallRect(maxWidth, (double) maxHeight, imageSize.Width, imageSize.Height) : ImageUtils.GetNormalRect(maxWidth, (double) maxHeight, imageSize.Width, imageSize.Height);
      viewModel.Width = size.Width;
      viewModel.Height = size.Height;
      viewModel.Tag = (object) attachmentMdDisplayModel;
      viewModel.SelectedBrush = flag2 ? this._editor.EditBox.TextArea.SelectionBrush : (Brush) null;
      viewModel.CornerRadius = flag4 ? new Thickness(4.0) : new Thickness(6.0);
      viewModel.StatusMargin = flag4 ? new Thickness(3.0) : new Thickness(6.0);
      double baselineExtra = 0.0;
      AttachmentImageDisplayControl element;
      if (flag3)
      {
        element = new AttachmentImageDisplayControl(viewModel, new AttachmentImageDisplayControl.OnAttachementMouseEnter(this._editor.OnAttachmentMouseEnter));
        if (attachmentMdDisplayModel.Attachment != null)
          this._cachedControls[attachmentMdDisplayModel.Attachment.id] = (FrameworkElement) element;
        else
          this._cachedControls[key] = (FrameworkElement) element;
      }
      else
        element = attachmentMdDisplayModel.Attachment == null ? this._cachedControls[key] as AttachmentImageDisplayControl : this._cachedControls[attachmentMdDisplayModel.Attachment.id] as AttachmentImageDisplayControl;
      return element != null ? new AttachmentElement(length, (UIElement) element, viewModel.Width + 0.0, viewModel.Height, baselineExtra, TextAlignment.Left) : (AttachmentElement) null;
    }

    private AttachmentElement GetFileElement(AttachmentInfo info)
    {
      AttachmentModel attachmentInUrl = AttachmentDao.GetAttachmentInUrl(info.Url);
      if (attachmentInUrl == null || attachmentInUrl.deleted)
        return (AttachmentElement) null;
      double width = Math.Max((this._editor.ActualWidth != 0.0 || double.IsNaN(this._editor.Width) ? this._editor.ActualWidth : this._editor.Width) - 30.0, 100.0);
      int height = 56;
      AttachmentFileDisplayControl fileControl = this.GetFileControl(attachmentInUrl, width, (double) height, new AttachmentImageDisplayControl.OnAttachementMouseEnter(this._editor.OnAttachmentMouseEnter));
      fileControl.Tag = (object) new AttachmentMDDisplayModel()
      {
        Info = info,
        Attachment = attachmentInUrl,
        LocalPath = attachmentInUrl.localPath
      };
      return new AttachmentElement(info.Value.Length, (UIElement) fileControl, width, (double) height, 0.0, TextAlignment.Center);
    }

    private AttachmentFileDisplayControl GetFileControl(
      AttachmentModel model,
      double width,
      double height,
      AttachmentImageDisplayControl.OnAttachementMouseEnter mouseEnterCallback)
    {
      AttachmentViewModel attachmentViewModel = this.GetAttachmentViewModel(model.id);
      bool flag = attachmentViewModel == null;
      AttachmentViewModel model1 = attachmentViewModel ?? new AttachmentViewModel(model);
      model1.Width = width;
      model1.Height = height;
      model1.Margin = this._editor.InPrint ? new Thickness(-12.0, 0.0, 0.0, 0.0) : new Thickness(0.0, 0.0, 0.0, 0.0);
      AttachmentFileDisplayControl fileControl;
      if (flag)
      {
        bool valueOrDefault = (this._editor?.FindResource((object) "IsDarkTheme") as bool?).GetValueOrDefault();
        fileControl = new AttachmentFileDisplayControl(model1, mouseEnterCallback, valueOrDefault);
        this._cachedControls[model.id] = (FrameworkElement) fileControl;
      }
      else
      {
        if (model1.AttachmentModel != null)
        {
          model1.AttachmentModel = model;
          model1.FileStatus = AttachmentLoadHelper.AttachmentStatus(model);
        }
        fileControl = this._cachedControls[model.id] as AttachmentFileDisplayControl;
      }
      return fileControl;
    }
  }
}
