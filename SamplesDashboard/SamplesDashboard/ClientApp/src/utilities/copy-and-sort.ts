// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Given an array of objects, sort by the specified column
export default function copyAndSort<T>(
  items: T[],
  columnKey: string,
  isSortedDescending?: boolean
): T[] {
  const key = columnKey as keyof T;
  return items
    .slice(0)
    .sort((a: T, b: T) => compare(a[key], b[key], isSortedDescending));
}

// Compare two objects for ordering purposes
function compare(a: any, b: any, isSortedDescending?: boolean) {
  // Handle the possible scenario of blank inputs
  // and keep them at the bottom of the lists
  if (!a) {
    return 1;
  }
  if (!b) {
    return -1;
  }

  let valueA: any;
  let valueB: any;
  let comparison = 0;

  if (typeof a === 'string' || a instanceof String) {
    // Use toUpperCase() to ignore character casing
    valueA = a.toUpperCase();
    valueB = b.toUpperCase();
    // its an item of type number
  } else if (typeof a === 'number' && typeof b === 'number') {
    valueA = a;
    valueB = b;
  } else if (a instanceof Array) {
    // Currently the only column that's an array
    // contains RepoOwner objects
    valueA = valueB = '';

    for (const value of a) {
      valueA = valueA + value.displayName;
    }

    for (const value of b) {
      valueB = valueB + value.displayName;
    }
  } else {
    // its an object which has a totalCount property
    valueA = a.totalCount;
    valueB = b.totalCount;
  }

  if (valueA > valueB) {
    comparison = 1;
  } else if (valueA < valueB) {
    comparison = -1;
  }

  if (isSortedDescending) {
    comparison = comparison * -1;
  }
  return comparison;
}
