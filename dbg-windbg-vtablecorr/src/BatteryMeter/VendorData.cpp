#include "stdafx.h"
#include "VendorData.h"

const int ACPI_VENDOR_ID = -499;

VendorData::~VendorData()
{
	// Give the hardware time to deinitialize
	::Sleep(5000);
}

int ACPIVendorData::GetVendorID()
{
	return ACPI_VENDOR_ID;
}
