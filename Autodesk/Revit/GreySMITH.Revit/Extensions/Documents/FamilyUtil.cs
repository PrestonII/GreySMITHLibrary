﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace GreySMITH.Revit.Commands.Extensions.Documents
{
    public static class FamilyUtil
    {
        /// <summary>
        /// Method designed to directly load a family into a project using only a family symbol
        /// </summary>
        /// <param name="curdoc"></param>
        /// <param name="famsym"></param>
        /// <param name="cmd">External Command Data from the current command</param>
        /// <param name="doctoloadfrom">Document the family symbol should be loaded from</param>
        public static FamilySymbol LoadFamilyDirect(this Document curdoc, FamilySymbol famsym, Document doctoloadfrom, ExternalCommandData cmd)
        {
            Family newfamily = null;
            // grab the application and change the active document
            using (UIApplication uiapp = cmd.Application)
            {
                OpenOptions oop = new OpenOptions();

                oop.AllowOpeningLocalByWrongUser = true;
                oop.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;

                // open "doctoloadfrom" (linked doc) and make it the active document
                if (uiapp.ActiveUIDocument.Document != doctoloadfrom)
                {
                    // try opening the document from memory only
                    try
                    {
                        Document newcurdoc = curdoc.Application.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(doctoloadfrom.PathName), oop);
                    }

                    // was unable to open the document for some reason
                    catch
                    {
                        uiapp.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(doctoloadfrom.PathName), oop, false);
                        throw new Exception("Failed in attempt to open file. File name is:" + doctoloadfrom.PathName);
                    }
                }

                // open the family document
                newfamily = famsym.Family;
                using (Document doc_family = doctoloadfrom.EditFamily(newfamily))
                {
                    // load it into the original document
                    doc_family.LoadFamily(curdoc);

                    // close the family once done
                    doc_family.Close(false);
                    //doctoloadfrom.Close(true);
                }

                // return control to the original active document
                // could create possible bug - write a method to both
                // move to another current document
                // close all but current document
                if (uiapp.ActiveUIDocument.Document != curdoc)
                {
                    while (uiapp.ActiveUIDocument.Document != curdoc)
                    {
                        uiapp.ActiveUIDocument.Document.Close(false);
                    }
                }

                using (Transaction tr_regen = new Transaction(curdoc, "Regenerating the document..."))
                {
                    tr_regen.Start();

                    // regenerate that document
                    curdoc.Regenerate();
                    tr_regen.Commit();
                }
            }

            return famsym;
        }

        public static void LoadFamilyDirect(this Document curdoc, FilteredElementCollector fec, ExternalCommandData excmd)
        {
            var collectionoffamsyms = from element in fec
                                      where element is FamilySymbol
                                      select element;

            foreach (FamilySymbol fs in collectionoffamsyms)
            {
                if (!curdoc.HasFamily(fs))
                {
                    FamilySymbol curfamysym = curdoc.LoadFamilyDirect(fs, fs.Document, excmd);
                }
            }
        }
        /// <summary>
        /// Returns truth value on whether the document contains the family symbol in question
        /// </summary>
        /// <param name="doc">Document to check</param>
        /// <param name="famsym">Family symbol to check for</param>
        /// <returns></returns>
        public static bool HasFamily(this Document doc, FamilySymbol famsym)
        {
            bool answer = false;
            List<Element> listoffams = new List<Element>();

            listoffams = doc.GetAllElements(famsym);
            var fammatches = from fam in listoffams
                             where fam.Name.Equals(famsym.Name)
                             select fam;

            if (fammatches.Count() > 0)
                answer = true;

            return answer;
        }

        //public static FamilySymbol LoadFamilyDirect(this Document curdoc, FamilySymbol famsym, Document doctoloadfrom, ExternalCommandData cmd)
        //{
        //    Family newfamily = null;
        //    // grab the application and change the active document
        //    using (UIApplication uiapp = cmd.Application)
        //    {
        //        OpenOptions oop = new OpenOptions();

        //        oop.AllowOpeningLocalByWrongUser = true;
        //        oop.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;

        //        // open "doctoloadfrom" (linked doc) and make it the active document
        //        if (uiapp.ActiveUIDocument.Document != doctoloadfrom)
        //        {
        //            // try opening the document from memory only
        //            try
        //            {
        //                Document newcurdoc = curdoc.Application.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(doctoloadfrom.PathName), oop);
        //            }

        //            // was unable to open the document for some reason
        //            catch
        //            {
        //                uiapp.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(doctoloadfrom.PathName), oop, false);
        //                throw new Exception("Failed in attempt to open file. File name is:" + doctoloadfrom.PathName);
        //            }
        //        }

        //        // open the family document
        //        newfamily = famsym.Family;
        //        using (Document doc_family = doctoloadfrom.EditFamily(newfamily))
        //        {
        //            // load it into the original document
        //            doc_family.LoadFamily(curdoc);

        //            // close the family once done
        //            doc_family.Close(false);
        //            //doctoloadfrom.Close(true);
        //        }

        //        // return control to the original active document
        //        // could create possible bug - write a method to both
        //        // move to another current document
        //        // close all but current document
        //        if (uiapp.ActiveUIDocument.Document != curdoc)
        //        {
        //            while (uiapp.ActiveUIDocument.Document != curdoc)
        //            {
        //                uiapp.ActiveUIDocument.Document.Close(false);
        //            }
        //        }

        //        using (Transaction tr_regen = new Transaction(curdoc, "Regenerating the document..."))
        //        {
        //            tr_regen.Start();

        //            // regenerate that document
        //            curdoc.Regenerate();
        //            tr_regen.Commit();
        //        }
        //    }

        //    return famsym;
        //}
    }

    //public static class FamilyExtender
    //{
    //    public static FamilySymbol LoadFamilyDirect(this Document curdoc, FamilySymbol famsym, Document doctoloadfrom, ExternalCommandData cmd)
    //    {
    //        Family newfamily = null;
    //        // grab the application and change the active document
    //        using (UIApplication uiapp = cmd.Application)
    //        {
    //            OpenOptions oop = new OpenOptions();

    //            oop.AllowOpeningLocalByWrongUser = true;
    //            oop.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;

    //            // open "doctoloadfrom" (linked doc) and make it the active document
    //            if (uiapp.ActiveUIDocument.Document != doctoloadfrom)
    //            {
    //                // try opening the document from memory only
    //                try
    //                {
    //                    Document newcurdoc = curdoc.Application.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(doctoloadfrom.PathName), oop);
    //                }

    //                // was unable to open the document for some reason
    //                catch
    //                {
    //                    uiapp.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(doctoloadfrom.PathName), oop, false);
    //                    throw new Exception("Failed in attempt to open file. File name is:" + doctoloadfrom.PathName);
    //                }
    //            }

    //            // open the family document
    //            newfamily = famsym.Family;
    //            using (Document doc_family = doctoloadfrom.EditFamily(newfamily))
    //            {
    //                // load it into the original document
    //                doc_family.LoadFamily(curdoc);

    //                // close the family once done
    //                doc_family.Close(false);
    //                //doctoloadfrom.Close(true);
    //            }

    //            // return control to the original active document
    //            // could create possible bug - write a method to both
    //            // move to another current document
    //            // close all but current document
    //            if (uiapp.ActiveUIDocument.Document != curdoc)
    //            {
    //                while (uiapp.ActiveUIDocument.Document != curdoc)
    //                {
    //                    uiapp.ActiveUIDocument.Document.Close(false);
    //                }
    //            }

    //            using (Transaction tr_regen = new Transaction(curdoc, "Regenerating the document..."))
    //            {
    //                tr_regen.Start();

    //                // regenerate that document
    //                curdoc.Regenerate();
    //                tr_regen.Commit();
    //            }
    //        }

    //        return famsym;
    //    }

    //    //public void OtherMethod()
    //    //{

    //    //}
    //}

    ////class NewFamilyClass : IFamilyExtender, FamilyUtil
    ////{

    ////}
}
