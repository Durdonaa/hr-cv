﻿//using DataTables.AspNet.Core;
//using DataTables.AspNet.Mvc5;
using INTRANET.Data;
using INTRANET.Data.Infrastructure;
using INTRANET.Data.Repository;
using INTRANET.Model;
using INTRANET.Service;
using INTRANET.Service.Interfaces;
using INTRANET.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INTRANET.Common;
using System.Reflection;
using Spire.Doc;
using System.Web.Hosting;

namespace INTRANET.Controllers
{
    public class HrCvController : Controller
    {
        public IHrEmployeeService _hrEmployeeService { get; set; }
        public IHrDepartmentService _hrDepartmentService { get; set; }
        public IHrPositionService _hrPositionService { get; set; }
        public IHrCvDetailService _hrCvDetailService { get; set; }
        public IHrCvEductionService _hrCvEducationService { get; set; }
        public IHrCvLaborService _hrCvLaborService { get; set; }
        public IHrCvRelativeService _hrCvRelativeService { get; set; }
        public IHrCvAwardService _hrCvAwardService { get; set; }
        public IHrCvMembershipService _hrCvMembershipService { get; set; }



        public HrCvController(IHrEmployeeService hrEmployeeService,
            IHrDepartmentService hrDepartmentService, IHrPositionService hrPositionService, IHrCvDetailService hrCvDetailService,
            IHrCvEductionService hrCvEducationService)
        {
            _hrEmployeeService = hrEmployeeService;
            _hrDepartmentService = hrDepartmentService;
            _hrPositionService = hrPositionService;
            _hrCvDetailService = hrCvDetailService;
            _hrCvEducationService = hrCvEducationService;
        }

        // GET: HrCv
        public ActionResult Index()
        {
            var departmentsList = _hrDepartmentService
                                    .GetAll()
                                    .Select(a => new SelectListItem
                                    {
                                        Text = a.TitleEn,
                                        Value = a.Id.ToString()
                                    }).ToList();

            var positionsList = _hrPositionService
                                    .GetAll()
                                    .Select(a => new SelectListItem
                                    {
                                        Text = a.TitleEn,
                                        Value = a.Id.ToString()
                                    }).ToList();
            var model = new HrCvListVM
            {
                Departments = departmentsList,
                Positions = positionsList
            };

            return View(model);
        }

        private HrEmployeeListVM MapToModel(HrEmployee hrEmployee)
        {
            return new HrEmployeeListVM
            {
                Id = hrEmployee.Id,
                FullName = hrEmployee.FullName,
                DepartmentName = hrEmployee.Department?.TitleEn,
                PositionName = hrEmployee.Position?.TitleEn,
                HasFilledRuCV = hrEmployee.ComplietedRuCv,
                HasFilledUzCV = hrEmployee.ComplietedUzCv
            };
        }


        public ActionResult LoadData(int[] selectedDepartments, int[] selectedPositions)
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                // Skiping number of Rows count  
                var start = Request.Form.GetValues("start").FirstOrDefault();
                // Paging Length 10,20  
                var length = Request.Form.GetValues("length").FirstOrDefault();
                // Sort Column Name  
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                // Sort Column Direction ( asc ,desc)  
                var sortColumnDirection = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                // Search Value from (Search box)  
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                //Paging Size (10,20,50,100)  
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Customer data  
                var employeeData = _hrEmployeeService.GetAllQueryable();

                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    switch (sortColumn)
                    {
                        case "FullName":
                            if (sortColumnDirection == "asc")
                                employeeData = employeeData.OrderBy(c => c.FullName);
                            else
                                employeeData = employeeData.OrderByDescending(c => c.FullName);
                            break;
                        case "DepartmentName":
                            if (sortColumnDirection == "asc")
                                employeeData = employeeData.OrderBy(c => c.Department.TitleEn);
                            else
                                employeeData = employeeData.OrderByDescending(c => c.Department.TitleEn);
                            break;
                        case "Position":
                            if (sortColumnDirection == "asc")
                                employeeData = employeeData.OrderBy(c => c.Position.TitleEn);
                            else
                                employeeData = employeeData.OrderByDescending(c => c.Position.TitleEn);
                            break;
                        case "HasFilledRuCV":
                            if (sortColumnDirection == "asc")
                                employeeData = employeeData.OrderBy(c => c.ComplietedRuCv);
                            else
                                employeeData = employeeData.OrderByDescending(c => c.ComplietedRuCv);
                            break;
                        case "HasFilledUzCV":
                            if (sortColumnDirection == "asc")
                                employeeData = employeeData.OrderBy(c => c.ComplietedUzCv);
                            else
                                employeeData = employeeData.OrderByDescending(c => c.ComplietedUzCv);
                            break;
                        default:
                            employeeData = employeeData.OrderBy(c => c.FullName);
                            break;
                    }
                }

                if (selectedDepartments != null && selectedDepartments.Any())
                    employeeData = employeeData.Where(
                        e => e.DepartmentId.HasValue &&
                        selectedDepartments.Contains(e.DepartmentId.Value));


                if (selectedPositions != null && selectedPositions.Any())
                    employeeData = employeeData.Where(
                        e => e.PositionId.HasValue &&
                        selectedPositions.Contains(e.PositionId.Value));



                ////Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    employeeData = employeeData.Where(m => m.FullName.Contains(searchValue));
                }

                //total number of rows count   
                recordsTotal = employeeData.Count();
                //Paging   
                //var data = employeeData.Skip(skip).Take(pageSize).ToList();
                var data = employeeData.Skip(skip).Take(pageSize).Select(MapToModel);
                //var data = employeeData.Take(pageSize).Select(MapToModel);

                //Returning Json Data  
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception ex)
            {
                //logging goes here
                return Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = new object[0] });
            }
        }

        [HttpPost]
        public ActionResult ChangeCvCompletionStatus(HrCvChangeCompletionStatusMode mode, int[] selectedEmployees)
        {
            try
            {
                _hrEmployeeService.ChangeCvCompletionStatus(mode, selectedEmployees);
                return Json(new { IsSuccess = true });
            }
            catch (Exception ex)
            {
                //logging goes here
                return Json(new { IsSuccess = false });
            }
        }

        [HttpGet]
        public ActionResult FillCv(int employeeId, HrCvLanguage language)
        {
            var employee = _hrEmployeeService.GetByID(employeeId);

            if (employee == null)
                return RedirectToAction("Index", "HrCv");

            var details = GetCvDetail(employeeId, language);

            var model = new HrCvVM
            {
                EmployeeId = employeeId,
                Language = language,
                Phone = employee.PhoneNo,
                ExternalPhone = employee.ExternalPhoneNo,
                EntryDate = employee.EntryDate,
                PlaceOfBirth = details.PlaceOfBirth,
                Nationality = details.Nationality,
                PartyMembership = details.PartyMembership,
                EducationDegree = details.EducationDegree,
                EducationSpeciality = details.EducationSpeciality,
                AcademicDegree = details.AcademicDegree,
                AcademicTitle = details.AcademicTitle,
                Languages = details.Languages,
                EducationList = _hrCvEducationService.GetForCvDetail(details.Id).Select(c => c.Education).ToList(),
                AwardList = _hrCvAwardService.GetForCvDetail(details.Id).Select(c => c.Award).ToList(),
                MembershipList = _hrCvMembershipService.GetForCvDetail(details.Id).Select(c => c.Membership).ToList(),
                LaborDetailList = _hrCvLaborService.GetForCvDetail(details.Id).Select(m => new HrCvLaborVM { Years = m.Years, Description = m.Description }).ToList(),
                RelativesDetailsList = _hrCvRelativeService.GetForCvDetail(details.Id).Select(c => new HrCvRelativesVM { Degree = c.Degree, FullName = c.FullName, BirthDateAndPlace = c.BirthDateAndPlace, LaborDetails = c.LaborDetails, Address = c.Address, }).ToList()
            };

            //add default first row
            if (!model.EducationList.Any())
                model.EducationList.Add("");
            if (!model.LaborDetailList.Any())
                model.LaborDetailList.Add(new HrCvLaborVM());
            if (!model.RelativesDetailsList.Any())
                model.RelativesDetailsList.Add(new HrCvRelativesVM());
            if (!model.AwardList.Any())
                model.AwardList.Add("");
            if (!model.MembershipList.Any())
                model.MembershipList.Add("");

            AddDefaultsToModel(model, employee);

            return View(model);
        }

        [HttpPost]
        public ActionResult FillCv(HrCvVM model, HttpPostedFileBase fileItem)
        {

            var employee = _hrEmployeeService.GetByID(model.EmployeeId);
            if (employee == null)
                return RedirectToAction("Index");

            AddDefaultsToModel(model, employee);


            //safety check to avoid null reference exceptions
            if (model.EducationList == null)
                model.EducationList = new List<string>();

            if (model.LaborDetailList == null)
                model.LaborDetailList = new List<HrCvLaborVM>();

            if (model.RelativesDetailsList == null)
                model.RelativesDetailsList = new List<HrCvRelativesVM>();

            var hasImageFile = fileItem != null && fileItem.ContentLength > 0;
            //validation for .png and jpg files 
            if (!hasImageFile)
            {
                //do not require image if previously there was one
                if (employee.ImageNameContent == null || employee.ImageNameContent.Length == 0) //previously no image
                    ModelState.AddModelError(string.Empty, "Photo was not selected");
            }
            else
            {
                string ex = Path.GetExtension(fileItem.FileName);
                List<string> acceptedExtensions = new List<string> { ".png", ".jpg" };

                if (!acceptedExtensions.Contains(ex))
                {
                    ModelState.AddModelError(string.Empty, "Photo must be in .png or .jpg format");
                }
            }

            if (ModelState.IsValid)
            {
                if (hasImageFile)
                {
                    byte[] data;
                    using (var inputStream = fileItem.InputStream)
                    {
                        var memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }
                        data = memoryStream.ToArray();
                        employee.ImageNameContent = data;
                        employee.ImageName = fileItem.FileName;
                        employee.ImageNameContentType = fileItem.ContentType;
                    }
                }
                employee.PhoneNo = model.Phone;
                employee.ExternalPhoneNo = model.ExternalPhone;


                try
                {
                    _hrEmployeeService.Update(employee);
                    var details = GetCvDetail(model.EmployeeId, model.Language);

                    details.Nationality = model.Nationality;
                    details.Languages = model.Languages;
                    details.PlaceOfBirth = model.PlaceOfBirth;
                    details.PartyMembership = model.PartyMembership;
                    details.AcademicDegree = model.AcademicDegree;
                    details.AcademicTitle = model.AcademicTitle;
                    details.EducationDegree = model.EducationDegree;
                    details.EducationSpeciality = model.EducationSpeciality;

                    _hrCvDetailService.Update(details);

                    _hrCvEducationService.Save(model.EducationList.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new HrCvEduction
                    {
                        Education = e
                    }).ToList(), details.Id);


                    _hrCvAwardService.Save(model.AwardList.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new HrCvAward
                    {
                        Award = e
                    }).ToList(), details.Id);


                    _hrCvLaborService.Save(model.LaborDetailList.Where(e =>
                    !string.IsNullOrWhiteSpace(e.Years) &&
                    !string.IsNullOrWhiteSpace(e.Description)).Select(e => new HrCvLabor
                    {
                        Years = e.Years,
                        Description = e.Description,
                    }).ToList(), details.Id);


                    _hrCvRelativeService.Save(model.RelativesDetailsList.Where(e =>
                    !string.IsNullOrWhiteSpace(e.Address) &&
                    !string.IsNullOrWhiteSpace(e.BirthDateAndPlace) &&
                    !string.IsNullOrWhiteSpace(e.Degree) &&
                    !string.IsNullOrWhiteSpace(e.FullName) &&
                    !string.IsNullOrWhiteSpace(e.LaborDetails)
                    ).Select(e => new HrCvRelative
                    {
                        Degree = e.Degree,
                        FullName = e.FullName,
                        BirthDateAndPlace = e.BirthDateAndPlace,
                        LaborDetails = e.LaborDetails,
                        Address = e.Address

                    }).ToList(), details.Id);

                    _hrCvMembershipService.Save(model.MembershipList.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new HrCvMembership
                    {
                        Membership = e
                    }).ToList(), details.Id);


                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Form filled incorrectly");

                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult DownloadCv(int employeeId, HrCvLanguage language)
        {
            var employee = _hrEmployeeService.GetByID(employeeId);

            if (employee == null)
                return RedirectToAction("Index", "HrCv");

            var employeeCV = _hrCvDetailService.GetForCv(employeeId, language);
            Document doc = new Document();
            string path;
            string filename;

            if (language == HrCvLanguage.Uz)
            {
                path = HostingEnvironment.MapPath("~/Content/HrCvTemplates/cv_template_uz.doc");
                filename = employee.FullName + "(Uz).doc";


            }
            else //only 2 possible cases
            {
                path = "cv_template_ru.doc";
                filename = employee.FullName + "(Ru).doc";
            }

            doc.LoadFromFile(path);
            doc.Replace("{FULLNAME}", employee.FullName, true, true);
            doc.Replace("{CURRENTPOSITIONDATE}", employee.PositionStartDate.Value.ToString("dd MMMM yyyy"), true, true);
            doc.Replace("{DATEOFBIRTH}", employee.DateOfBirth.ToString("MM/dd/yyyy"), true, true);
            doc.Replace("{PLACEOFBIRTH}", employee.PlaceOfBirth.ToString(), true, true);
            doc.Replace("{NATIONALITY}", employeeCV?.Nationality ?? "", true, true);
            doc.Replace("{PARTYMEMBERSHIP}", employeeCV?.PartyMembership ?? "", true, true);
            doc.Replace("{EDUCATIONDEGREE}", employeeCV?.EducationDegree ?? "", true, true);
            doc.Replace("{EDUCATIONSPECIALITY}", employeeCV?.EducationSpeciality ?? "", true, true);
            doc.Replace("{ACADEMICDEGREE}", employeeCV?.AcademicDegree ?? "", true, true);
            doc.Replace("{ACADEMICTITLE}", employeeCV?.AcademicTitle ?? "", true, true);
            doc.Replace("{LANGUAGES}", employeeCV?.Languages ?? "", true, true);
            doc.Replace("{AWARDS}", employeeCV?.Awards.ToString() ?? "", true, true);
            doc.Replace("{MEMBERSHIPS}", employeeCV?.Memberships.ToString() ?? "", true, true);

            Table table = doc.Sections[0].Tables[0] as Spire.Doc.Table;
            //Insert a new row at the bottom of the table
            table.AddRow(true, 5);
            table.Rows[2].Cells[1].Paragraphs[0].AppendText("TEST TEST TEST TEST");

            filename = filename.Replace(@"\", " ")
                            .Replace(@"/", " ")
                            .Replace(@":", " ")
                            .Replace(@"*", " ")
                            .Replace(@"?", " ")
                            .Replace("\"", " ")
                            .Replace(@"<", " ")
                            .Replace(@">", " ")
                            .Replace(@"|", " ")
                            .Replace(@"  ", " ");

            doc.SaveToFile(filename, FileFormat.Doc);

            byte[] data = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                doc.SaveToStream(memoryStream, FileFormat.Doc);
                //save to byte array
                data = memoryStream.ToArray();
            }

            return File(data, "application/msword", filename);

        }

        private HrCvDetail GetCvDetail(int employeeId, HrCvLanguage language)
        {
            var details = _hrCvDetailService.GetForCv(employeeId, language);

            //did not fill CV yet, create record
            if (details == null)
            {
                details = new HrCvDetail
                {
                    EmployeeId = employeeId,
                    Language = language
                };

                _hrCvDetailService.Create(details);
            }
            return details;
        }

        //add readonly fields
        private void AddDefaultsToModel(HrCvVM model, HrEmployee employee)
        {
            model.EmployeeName = employee.FullName;
            model.DepartmentName = model.Language == HrCvLanguage.Ru ? employee.Department.TitleRu : employee.Department.TitleUz;
            model.PositionName = model.Language == HrCvLanguage.Ru ? employee.Position.TitleRu : employee.Position.TitleUz;
            model.DateOfBirth = employee.DateOfBirth;
            if (employee.ImageNameContent != null && employee.ImageNameContent.Length > 0)
            {
                model.ImageContent = Convert.ToBase64String(employee.ImageNameContent);
            }
        }

    }
}