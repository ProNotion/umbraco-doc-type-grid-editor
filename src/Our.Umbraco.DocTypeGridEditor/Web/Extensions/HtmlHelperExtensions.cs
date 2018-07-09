﻿using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Our.Umbraco.DocTypeGridEditor.Extensions;
using Our.Umbraco.DocTypeGridEditor.Web.Helpers;
using Our.Umbraco.DocTypeGridEditor.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Our.Umbraco.DocTypeGridEditor.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static HtmlString RenderDocTypeGridEditorItem(
            this HtmlHelper helper,
            IPublishedContent content,
            string editorAlias = "",
            string viewPath = "",
            string previewViewPath = "",
            bool isPreview = false)
        {
            if (content == null)
                return new HtmlString(string.Empty);

            var controllerName = $"{content.DocumentTypeAlias}Surface";

            if (string.IsNullOrWhiteSpace(viewPath) == false)
                viewPath = viewPath.EnsureEndsWith('/');

            if (string.IsNullOrWhiteSpace(previewViewPath) == false)
                previewViewPath = previewViewPath.EnsureEndsWith('/');

            // Try looking for surface controller with action named after the editor alias
            if (string.IsNullOrWhiteSpace(editorAlias) == false && SurfaceControllerHelper.SurfaceControllerExists(controllerName, editorAlias, true))
            {
                return helper.Action(editorAlias, controllerName, new
                {
                    dtgeModel = content,
                    dtgeViewPath = viewPath,
                    dtgePreviewViewPath = previewViewPath
                });
            }

            // Try looking for surface controller with action named after the doc type alias alias
            if (SurfaceControllerHelper.SurfaceControllerExists(controllerName, content.DocumentTypeAlias, true))
            {
                return helper.Action(content.DocumentTypeAlias, controllerName, new
                {
                    dtgeModel = content,
                    dtgeViewPath = viewPath,
                    dtgePreviewViewPath = previewViewPath
                });
            }

            // See if a default surface controller has been registered
            var defaultController = DefaultDocTypeGridEditorSurfaceControllerResolver.Current.GetDefaultControllerType();
            if (defaultController != null)
            {
                var defaultControllerName = defaultController.Name.Substring(0, defaultController.Name.LastIndexOf("Controller"));

                // Try looking for an action named after the editor alias
                if (string.IsNullOrWhiteSpace(editorAlias) == false && SurfaceControllerHelper.SurfaceControllerExists(defaultControllerName, editorAlias, true))
                {
                    return helper.Action(editorAlias, defaultControllerName, new
                    {
                        dtgeModel = content,
                        dtgeViewPath = viewPath,
                        dtgePreviewViewPath = previewViewPath
                    });
                }

                // Try looking for a doc type alias action
                if (SurfaceControllerHelper.SurfaceControllerExists(defaultControllerName, content.DocumentTypeAlias, true))
                {
                    return helper.Action(content.DocumentTypeAlias, defaultControllerName, new
                    {
                        dtgeModel = content,
                        dtgeViewPath = viewPath,
                        dtgePreviewViewPath = previewViewPath
                    });
                }

                // Just go with a default action name
                return helper.Action("Index", defaultControllerName, new
                {
                    dtgeModel = content,
                    dtgeViewPath = viewPath,
                    dtgePreviewViewPath = previewViewPath
                });
            }

            // Check for preview view
            if (string.IsNullOrWhiteSpace(previewViewPath) == false
                && isPreview)
            {
                var fullPreviewViewPath = $"{previewViewPath}{editorAlias}.cshtml";
                if (ViewEngines.Engines.ViewExists(helper.ViewContext, fullPreviewViewPath, true))
                {
                    return helper.Partial(fullPreviewViewPath, content);
                }

                fullPreviewViewPath = $"{previewViewPath}{content.DocumentTypeAlias}.cshtml";
                if (ViewEngines.Engines.ViewExists(helper.ViewContext, fullPreviewViewPath, true))
                {
                    return helper.Partial(fullPreviewViewPath, content);
                }

                fullPreviewViewPath = $"{previewViewPath}Default.cshtml";
                if (ViewEngines.Engines.ViewExists(helper.ViewContext, fullPreviewViewPath, true))
                {
                    return helper.Partial(fullPreviewViewPath, content);
                }
            }

            // Check for view path view
            if (string.IsNullOrWhiteSpace(viewPath) == false)
            {
                var fullViewPath = $"{viewPath}{editorAlias}.cshtml";
                if (ViewEngines.Engines.ViewExists(helper.ViewContext, fullViewPath, true))
                {
                    return helper.Partial(fullViewPath, content);
                }

                fullViewPath = $"{viewPath}{content.DocumentTypeAlias}.cshtml";
                if (ViewEngines.Engines.ViewExists(helper.ViewContext, fullViewPath, true))
                {
                    return helper.Partial(fullViewPath, content);
                }

                fullViewPath = $"{viewPath}Default.cshtml";
                if (ViewEngines.Engines.ViewExists(helper.ViewContext, fullViewPath, true))
                {
                    return helper.Partial(fullViewPath, content);
                }
            }

            // Resort to standard partial views
            if (ViewEngines.Engines.ViewExists(helper.ViewContext, editorAlias, true))
            {
                return helper.Partial(editorAlias, content);
            }

            return helper.Partial(content.DocumentTypeAlias, content);
        }
    }
}