﻿using AutoMapper;
using HospitalAPI.Models.DataModels;
using HospitalAPI.Models.DbContextModel;
using HospitalAPI.Models.DTOs;
using HospitalAPI.Models.ViewModels;
using HospitalAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace HospitalAPI.Repositories
{
    public class PatientRepository : IPatientService
    {
        private readonly MyDbContext _db;
        private readonly IMapper _mapper;
        public PatientRepository(MyDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<int> Add(PatientDTO entity)
        {
            try
            {
                Patient patient = _mapper.Map<Patient>(entity);

                await _db.Patients.AddAsync(patient);
                await _db.SaveChangesAsync();

                var doctorPatient = new DoctorPatient
                {
                    DoctorId = entity.DoctorId,
                    PatientId = patient.Id
                };
                await _db.DoctorPatients.AddAsync(doctorPatient);
                return await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteById(int Id)
        {
            try
            {
                var record = await _db.Patients.SingleOrDefaultAsync(x => x.Id == Id);
                if (record == null) return 0;
                _db.Patients.Remove(record);
                return await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Patient>> GetAll(int skip, int take)
        {
            return await _db.Patients
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Patient?> GetById(int Id)
        {

            return await _db.Patients
                .SingleOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<IEnumerable<Patient>> GetPatientsAtDepartment(int departmentId)
        {
            return await _db.Patients.Where(p => p.DoctorPatients.Any(dp => dp.Doctor.DepartmentId == departmentId))
            .ToListAsync();
        }

        public async Task<IEnumerable<Patient>> GetPatientsAtDoctor(int doctorId)
        {
            return await _db.Patients.Where(p => p.DoctorPatients.Any(x => x.Doctor.Id == doctorId))
                .ToListAsync();
        }
        public async Task<PateintAppointmentViewModel?> GetPateintAtAppointment(int appointmentId)
        {
            return (from DP in _db.DoctorPatients
                    join P in _db.Patients
                    on DP.PatientId equals P.Id
                    join D in _db.Doctors
                    on DP.DoctorId equals D.Id
                    join Dept in _db.Departments
                    on D.DepartmentId equals Dept.Id
                    join A in _db.Appointments
                    on DP.Id equals A.DoctorPatientId
                    join B in _db.Billings
                    on A.Id equals B.AppointmentId
                    where A.Id == appointmentId
                    select new PateintAppointmentViewModel
                    {
                        PatientName = P.FullName,
                        PatientGender = P.Gender.Value,
                        DoctorName = D.FullName,
                        DoctorDepartment = D.Department.Name,
                        DoctorGender = D.Gender.Value,
                        AppointmentDate = A.DateTime,
                        BillingAmount = B.Amount,
                        BillingStatus = B.Status
                    }).FirstOrDefault();
        }
        public async Task<int> Update(int Id, PatientDTO entity)
        {
            try
            {
                var record = await _db.Patients.SingleOrDefaultAsync(x => x.Id == Id);
                if (record == null) return 0;

                if (!string.IsNullOrEmpty(entity.FullName)) record.FullName = entity.FullName;
                if (!string.IsNullOrEmpty(entity.Email)) record.Email = entity.Email;
                if (!string.IsNullOrEmpty(entity.Phone)) record.Phone = entity.Phone;
                if (!string.IsNullOrEmpty(entity.Address)) record.Address = entity.Address;
                if (entity.Gender.HasValue) record.Gender = entity.Gender;

                return await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
