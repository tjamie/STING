# Spatial Tools for the Integration of Nature-related Geoprocessing (STING)

## About

STING is an ArcGIS Pro add-in written to streamline the process of fetching vector data from various agencies' servers directly within the ArcGIS Pro environment. The current version of STING allows users to easily retrieve data from the following agencies:
- Natural Resources Conservation Service (NRCS) - Soils Data
- National Wetlands Inventory (NWI) - Wetlands Data
- Federal Emergency Management Agency (FEMA) - Floodplain Data

## Usage

Once installed, this add-in adds a tab to the ribbon. The STING tab contains a button corresponding to each of the above-referenced entities.

Upon clicking one of this add-in's buttons, the user is prompted to provide a polygon. This item is used to determine the spatial extent of vector data being requested. The current version of STING requires this feature to be within a geodatabase.

Once a valid polygon is selected, STING creates an HTTP request to the corresponding agency's GIS server. If vector data is provided in the HTTP response, STING converts the relevant GeoJSON or GML data to a feature class. The current version of STING saves this feature class to the current project's default geodatabase.