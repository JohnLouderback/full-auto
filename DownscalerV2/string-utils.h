#pragma once

#include <string>
#include <vector>
#include <cstdlib>
#include <algorithm>
#include <cwctype>

/**
 * @brief Converts a `std::string` to a `std::wstring`. That is, a multibyte string to a wide character string.
 * @param str The string to convert.
 * @returns The wide character string.
 */
inline std::wstring StringToWString(const std::string& str) {
  // If the string is empty, return an empty string.
  if (str.empty()) {
    return {};
  }

  // Determine the size needed for the wide character string. This is necessary because the size of the wide character
  // string may be larger than the size of the multibyte string.
  int sizeNeeded = MultiByteToWideChar(CP_UTF8, 0, &str[0], static_cast<int>(str.size()), nullptr, 0);

  // Create the wide character string and return it.
  std::wstring wstr(sizeNeeded, 0);
  MultiByteToWideChar(CP_UTF8, 0, &str[0], static_cast<int>(str.size()), &wstr[0], sizeNeeded);
  return wstr;
}

/**
 * @brief Given two wide strings, compares them in a case-insensitive manner. So "Hello" and "hello" would be considered
 *        equal.
 * @param a The first wide string to compare.
 * @param b The second wide string to compare.
 * @returns `true` if the strings are equal, `false` otherwise.
 */
inline bool InsensitiveComparison(const std::wstring& a, const std::wstring& b) {
  // If the sizes of the strings are different, they cannot be equal.
  if (a.size() != b.size()) {
    return false;
  }

  // Compare the strings in a case-insensitive manner by converting each character to lowercase and comparing them.
  return std::equal(
    a.begin(),
    a.end(),
    b.begin(),
    b.end(),
    [](wchar_t a, wchar_t b) {
      return std::towlower(a) == std::towlower(b);
    }
  );
}
